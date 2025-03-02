// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.Lambda.TestUtilities;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Extensions.Caching;
using Amazon.SecretsManager.Model;
using JustEat.HttpClientInterception;
using MartinCostello.LondonTravel.Skill.Extensions;
using MartinCostello.LondonTravel.Skill.Models;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace MartinCostello.LondonTravel.Skill;

public class AlexaFunctionTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    [Fact]
    public async Task Cannot_Invoke_Function_If_Application_Id_Incorrect()
    {
        // Arrange
        var function = await CreateFunctionAsync();
        var context = new TestLambdaContext();

        var request = CreateIntentRequest("AMAZON.HelpIntent");
        request.Session.Application.ApplicationId = "not-my-skill-id";

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => function.HandlerAsync(request, context));

        // Assert
        exception.Message.ShouldBe("Request application Id 'not-my-skill-id' and configured skill Id 'my-skill-id' mismatch.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("____")]
    [InlineData("qps-Ploc")]
    public async Task Can_Invoke_Function_If_Locale_Is_Invalid(string? locale)
    {
        // Arrange
        var function = await CreateFunctionAsync();
        var context = new TestLambdaContext();

        var request = CreateIntentRequest("AMAZON.HelpIntent");
        request.Request.Locale = locale!;

        // Act
        var actual = await function.HandlerAsync(request, context);

        // Assert
        var response = AssertResponse(actual, shouldEndSession: false);

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
    }

    [Fact]
    public async Task Cannot_Invoke_Function_With_System_Failure()
    {
        // Arrange
        var function = await CreateFunctionAsync();
        var context = new TestLambdaContext();

        var error = new SystemExceptionRequest()
        {
            Error = new()
            {
                Message = "Internal error.",
                Type = AlexaErrorType.InternalError,
            },
            ErrorCause = new()
            {
                RequestId = "my-request-id",
            },
        };

        var request = CreateRequest(error);

        // Act
        var actual = await function.HandlerAsync(request, context);

        // Assert
        var response = AssertResponse(actual);

        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
        response.OutputSpeech.Ssml.ShouldBe("<speak>Sorry, something went wrong.</speak>");
    }

    [Fact]
    public async Task Can_Invoke_Function_If_Skill_Id_Valid()
    {
        // Arrange
        var secretsManager = Substitute.For<IAmazonSecretsManager>();

        ConfigureSecret(secretsManager, "alexa-london-travel/OTEL_EXPORTER_OTLP_HEADERS", "Authorization secret-key");
        ConfigureSecret(secretsManager, "alexa-london-travel/Skill__SkillId", "secret-skill-id");
        ConfigureSecret(secretsManager, "alexa-london-travel/Skill__TflApplicationKey", "secret-tfl-app-id");
        ConfigureSecret(secretsManager, "alexa-london-travel/Skill__TflApplicationId", "secret-tfl-app-key");

        using var cache = new SecretsManagerCache(secretsManager);
        using var function = new TestAlexaFunctionWithSecrets(cache, Interceptor, OutputHelper);

        _ = await function.InitializeAsync();

        var context = new TestLambdaContext();

        var request = CreateIntentRequest("AMAZON.HelpIntent");
        request.Session.Application.ApplicationId = "secret-skill-id";

        // Act
        var actual = await function.HandlerAsync(request, context);

        // Assert
        var response = AssertResponse(actual, shouldEndSession: false);

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");

        static void ConfigureSecret(IAmazonSecretsManager secretsManager, string id, string value)
        {
            var description = new DescribeSecretResponse()
            {
                ARN = $"arn:aws:secretsmanager:eu-west-1:01234567890:secret:{id}",
                VersionIdsToStages = new Dictionary<string, List<string>>()
                {
                    ["1"] = ["AWSCURRENT"],
                },
            };

            secretsManager.DescribeSecretAsync(Arg.Is<DescribeSecretRequest>((p) => p.SecretId == id), Arg.Any<CancellationToken>())
                          .Returns(Task.FromResult(description));

            secretsManager.GetSecretValueAsync(Arg.Is<GetSecretValueRequest>((p) => p.SecretId == id), Arg.Any<CancellationToken>())
                          .Returns(Task.FromResult(new GetSecretValueResponse() { SecretString = value }));
        }
    }

    protected sealed class TestAlexaFunctionWithSecrets(
        SecretsManagerCache cache,
        HttpClientInterceptorOptions interceptor,
        ITestOutputHelper? outputHelper) : TestAlexaFunction(interceptor, outputHelper)
    {
        protected override void Configure(ConfigurationBuilder builder)
        {
            base.Configure(builder);
            builder.AddSecretsManager(cache);
        }
    }
}
