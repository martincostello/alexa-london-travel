# AGENTS.md

This file provides guidance to AI coding agents when working with code in this repository.

## Overview

An Amazon Alexa skill for checking London travel status. It integrates with the [TfL API](https://api.tfl.gov.uk/)
to query the status of the London Underground, Overground, DLR, and Elizabeth line.

The skill is deployed as an AWS Lambda function written in C# with Native AOT compilation.

The solution targets the version of .NET specified in the global.json file and uses Central Package Management.

## Commands

### Build and Test

```powershell
./build.ps1
```

This script builds, tests, and publishes the solution. On Linux, it also compiles and tests the Native AOT binary.

### Individual dotnet commands

```bash
# Build
dotnet build

# Run unit tests
dotnet test test/LondonTravel.Skill.Tests/LondonTravel.Skill.Tests.csproj

# Run a single test (filter by name)
dotnet test test/LondonTravel.Skill.Tests/LondonTravel.Skill.Tests.csproj --filter "FullyQualifiedName~TestClassName"

# Run AppHost integration tests
dotnet test test/LondonTravel.Skill.AppHostTests/LondonTravel.Skill.AppHostTests.csproj

# Run end-to-end tests
dotnet test test/LondonTravel.Skill.EndToEndTests/LondonTravel.Skill.EndToEndTests.csproj

# Publish (standard)
dotnet publish src/LondonTravel.Skill/LondonTravel.Skill.csproj

# Publish for AWS Lambda (Native AOT, linux-arm64) - Linux only
dotnet publish src/LondonTravel.Skill/LondonTravel.Skill.csproj /p:PublishForAWSLambda=true

# Run benchmarks
./benchmark.ps1
```

### Local development with .NET Aspire

The `LondonTravel.Skill.AppHost` project uses .NET Aspire to run the Lambda function locally via the AWS Lambda Test Tool. Run it with:

```bash
dotnet run --project src/LondonTravel.Skill.AppHost
```

Required environment variables / user secrets (set via `dotnet user-secrets` in the AppHost project):

- `Skill:SkillApiUrl` - URL of the London Travel skill API
- `Skill:TflApiUrl` - URL of the TfL API
- `Skill:TflApplicationId` - TfL API application ID
- `Skill:TflApplicationKey` - TfL API application key
- `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_ENDPOINT_URL_SECRETS_MANAGER` (optional, for Secrets Manager)

## Architecture

### Project Structure

- **`src/LondonTravel.Skill`** - The main Lambda function (Native AOT, targets `linux-arm64` for deployment)
- **`src/LondonTravel.Skill.AppHost`** - .NET Aspire host for local development using AWS Lambda Test Tool
- **`test/LondonTravel.Skill.Tests`** - Unit tests using xUnit v3, Shouldly, NSubstitute, JustEat.HttpClientInterception, and MartinCostello.Testing.AwsLambdaTestServer
- **`test/LondonTravel.Skill.AppHostTests`** - Integration tests for the Aspire AppHost
- **`test/LondonTravel.Skill.EndToEndTests`** - End-to-end tests
- **`test/LondonTravel.Skill.NativeAotTests`** - Tests that are compiled and run as a Native AOT binary
- **`perf/LondonTravel.Skill.Benchmarks`** - BenchmarkDotNet benchmarks

### Request Flow

1. `FunctionEntrypoint` - Lambda entry point, bootstraps `AlexaFunction`
2. `AlexaFunction` - DI container setup; handles `SkillRequest` → `SkillResponse` with OpenTelemetry tracing
3. `FunctionHandler` - Verifies skill ID, sets culture from locale, routes by request type
4. `AlexaSkill` - Dispatches to the appropriate handler method (`OnIntentAsync`, `OnLaunch`, `OnSessionEnded`, `OnError`)
5. `IntentFactory` - Maps intent name to `IIntent` implementation
6. `IIntent` implementations in `Intents/`:
   - `StatusIntent` - Queries TfL for a specific line's status
   - `DisruptionIntent` - Queries TfL for disruptions across all lines
   - `CommuteIntent` - Retrieves user's preferred lines via `SkillClient` and checks their status
   - `HelpIntent`, `EmptyIntent`, `UnknownIntent` - Non-API intents

### External API Clients

- **`TflClient`** - Calls the TfL Unified API for line statuses
- **`SkillClient`** - Calls the companion London Travel skill API to retrieve a user's saved favourite lines (requires Alexa account linking token)

### Key Design Decisions

- **Native AOT**: The skill uses `PublishAot=true` and source-generated JSON serialization (`AppJsonSerializerContext`) to support AOT compilation. No reflection-based JSON is used.
- **Response building**: `SkillResponseBuilder` provides a fluent API for constructing Alexa `SkillResponse` objects.
- **Localization**: Response strings are in `Strings.resx` (en-GB default) with locale variants. `CultureSwitcher` temporarily sets the thread culture based on the Alexa request locale.
- **Configuration**: `SkillConfiguration` is bound from the `"Skill"` config section. In AWS Lambda, secrets are loaded from AWS Secrets Manager via `SecretsManagerConfigurationProvider`.
- **Resilience**: HTTP clients use Polly for retry/resilience policies configured in `IHttpClientBuilderExtensions`.
- **Telemetry**: OpenTelemetry is used for tracing (via `AWSLambdaWrapper`) and logging.

### Code Style

- Follows EditorConfig settings (`.editorconfig` at repo root)
- StyleCop is configured via `stylecop.json`
- Namespace root: `MartinCostello.LondonTravel.Skill`
- Test assertions use Shouldly; mocking uses NSubstitute
- Code coverage threshold: enforced by coverlet, should be greater than 85%
