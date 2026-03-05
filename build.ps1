#! /usr/bin/env pwsh
param(
    [Parameter(Mandatory = $false)][string] $Configuration = "Release",
    [Parameter(Mandatory = $false)][switch] $SkipTests,
    [Parameter(Mandatory = $false)][switch] $UseManagedRuntime
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

$solutionPath = $PSScriptRoot
$sdkFile = Join-Path $solutionPath "global.json"

$dotnetVersion = (Get-Content $sdkFile | Out-String | ConvertFrom-Json).sdk.version
$installDotNetSdk = $false

if (($null -eq (Get-Command "dotnet" -ErrorAction SilentlyContinue)) -and ($null -eq (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue))) {
    Write-Information "The .NET SDK is not installed."
    $installDotNetSdk = $true
}
else {
    Try {
        $installedDotNetVersion = (dotnet --version 2>&1 | Out-String).Trim()
    }
    Catch {
        $installedDotNetVersion = "?"
    }

    if ($installedDotNetVersion -ne $dotnetVersion) {
        Write-Information "The required version of the .NET SDK is not installed. Expected $dotnetVersion."
        $installDotNetSdk = $true
    }
}

if ($installDotNetSdk -eq $true) {

    ${env:DOTNET_INSTALL_DIR} = Join-Path $solutionPath ".dotnet"
    $sdkPath = Join-Path ${env:DOTNET_INSTALL_DIR} "sdk" $dotnetVersion

    if (-Not (Test-Path $sdkPath)) {
        if (-Not (Test-Path ${env:DOTNET_INSTALL_DIR})) {
            mkdir ${env:DOTNET_INSTALL_DIR} | Out-Null
        }
        [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor "Tls12"

        if (($PSVersionTable.PSVersion.Major -ge 6) -And (-Not $IsWindows)) {
            $installScript = Join-Path ${env:DOTNET_INSTALL_DIR} "install.sh"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.sh" -OutFile $installScript -UseBasicParsing
            chmod +x $installScript
            & $installScript --jsonfile $sdkFile --install-dir ${env:DOTNET_INSTALL_DIR} --no-path --skip-non-versioned-files
        }
        else {
            $installScript = Join-Path ${env:DOTNET_INSTALL_DIR} "install.ps1"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
            & $installScript -JsonFile $sdkFile -InstallDir ${env:DOTNET_INSTALL_DIR} -NoPath -SkipNonVersionedFiles
        }
    }
}
else {
    ${env:DOTNET_INSTALL_DIR} = Split-Path -Path (Get-Command dotnet).Path
}

$dotnet = Join-Path ${env:DOTNET_INSTALL_DIR} "dotnet"

if ($installDotNetSdk) {
    ${env:PATH} = "${env:DOTNET_INSTALL_DIR};${env:PATH}"
}

function DotNetTest {
    param([string]$Project)

    $additionalArgs = @()

    if (-Not [string]::IsNullOrEmpty(${env:GITHUB_SHA})) {
        $additionalArgs += "--logger:GitHubActions;report-warnings=false"
        $additionalArgs += "--logger:junit;LogFilePath=junit.xml"
        $additionalArgs += "--blame-hang"
        $additionalArgs += "--blame-hang-timeout"
        $additionalArgs += "180s"
    }

    & $dotnet test $Project --configuration $Configuration $additionalArgs

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code ${LASTEXITCODE}"
    }
}

function DotNetPublish {
    param([string]$Project, [bool]$PublishForAWSLambda = $false)

    $additionalArgs = @()

    if ($PublishForAWSLambda) {
        $additionalArgs += "/p:PublishForAWSLambda=true"
    }

    & $dotnet publish $Project $additionalArgs

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code ${LASTEXITCODE}"
    }
}

$testProjects = @(
    (Join-Path $solutionPath "test" "LondonTravel.Skill.Tests" "LondonTravel.Skill.Tests.csproj"),
    (Join-Path $solutionPath "test" "LondonTravel.Skill.AppHostTests" "LondonTravel.Skill.AppHostTests.csproj"),
    (Join-Path $solutionPath "test" "LondonTravel.Skill.EndToEndTests" "LondonTravel.Skill.EndToEndTests.csproj")
)

$testProjectsForAot = @(
    (Join-Path $solutionPath "test" "LondonTravel.Skill.NativeAotTests" "LondonTravel.Skill.NativeAotTests.csproj")
)

$publishProjects = @(
    (Join-Path $solutionPath "src" "LondonTravel.Skill" "LondonTravel.Skill.csproj")
)

Write-Information "Publishing solution..."
$publishForAWSLambda = $IsLinux -And (-Not $UseManagedRuntime)
ForEach ($project in $publishProjects) {
    DotNetPublish $project $publishForAWSLambda
}

if (-Not $SkipTests) {
    Write-Information "Testing $($testProjects.Count) project(s)..."
    ForEach ($project in $testProjects) {
        DotNetTest $project
    }

    Write-Information "Testing $($testProjectsForAot.Count) project(s) for native AoT..."
    ForEach ($project in $testProjectsForAot) {
        DotNetPublish $project

        $projectName = [System.IO.Path]::GetDirectoryName($project)
        $projectName = [System.IO.Path]::GetFileName($projectName)
        $testBinary = (Join-Path $solutionPath "artifacts" "publish" $projectName $Configuration.ToLowerInvariant() $projectName)

        & $testBinary

        if ($LASTEXITCODE -ne 0) {
            throw "Native AoT tests for $projectName failed with exit code ${LASTEXITCODE}"
        }
    }
}
