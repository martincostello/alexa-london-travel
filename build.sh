#!/usr/bin/env bash

root=$(cd "$(dirname "$0")"; pwd -P)
artifacts=$root/artifacts
configuration=Release

while :; do
    if [ $# -le 0 ]; then
        break
    fi

    lowerI="$(echo $1 | awk '{print tolower($0)}')"
    case $lowerI in
        -\?|-h|--help)
            echo "./build.sh [--output <OUTPUT_DIR>]"
            exit 1
            ;;

        --output)
            artifacts="$2"
            shift
            ;;

        *)
            __UnprocessedBuildArgs="$__UnprocessedBuildArgs $1"
            ;;
    esac

    shift
done

export CLI_VERSION=`cat ./global.json | grep -E '[0-9]\.[0-9]\.[a-zA-Z0-9\-]*' -o`
export DOTNET_INSTALL_DIR="$root/.dotnetcli"
export PATH="$DOTNET_INSTALL_DIR:$PATH"

dotnet_version=$(dotnet --version)

if [ "$dotnet_version" != "$CLI_VERSION" ]; then
    curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version "$CLI_VERSION" --install-dir "$DOTNET_INSTALL_DIR"
fi

dotnet build ./LondonTravel.Skill.sln --output $artifacts --configuration $configuration || exit 1

dotnet test ./test/LondonTravel.Skill.Tests/LondonTravel.Skill.Tests.csproj --output $artifacts --configuration $configuration || exit 1

dotnet publish ./src/LondonTravel.Skill/LondonTravel.Skill.csproj --output $artifacts/publish --configuration $configuration --runtime linux-x64 --self-contained true /p:AssemblyName=bootstrap /p:PublishReadyToRun=true || exit 1

zip $artifacts/alexa-london-travel.zip $artifacts/publish || exit 1
