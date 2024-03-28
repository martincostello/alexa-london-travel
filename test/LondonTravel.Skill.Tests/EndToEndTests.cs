// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using MartinCostello.LondonTravel.Skill.Models;
using MartinCostello.Testing.AwsLambdaTestServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill;

public class EndToEndTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    [xRetry.RetryFact]
    public async Task Alexa_Function_Can_Process_Intent_Request()
    {
        // Arrange
        var intent = new IntentRequest()
        {
            Intent = new() { Name = "AMAZON.CancelIntent" },
        };

        SkillRequest request = CreateRequest(intent);

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        ResponseBody response = AssertResponse(actual);

        response.Card.ShouldBeNull();
        response.OutputSpeech.ShouldBeNull();
        response.Reprompt.ShouldBeNull();
    }

    [xRetry.RetryFact]
    public async Task Alexa_Function_Can_Process_Launch_Request()
    {
        // Arrange
        SkillRequest request = CreateRequest<LaunchRequest>();

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        ResponseBody response = AssertResponse(actual, shouldEndSession: false);

        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");

        var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
        ssml.Ssml.ShouldBe("<speak>Welcome to London Travel. You can ask me about disruption or for the status of any tube line, London Overground, the D.L.R. or the Elizabeth line.</speak>");
    }

    [xRetry.RetryFact]
    public async Task Alexa_Function_Can_Process_Session_Ended_Request()
    {
        // Arrange
        var session = new SessionEndedRequest()
        {
            Reason = Reason.ExceededMaxReprompts,
            Error = new()
            {
                Message = "Too many requests.",
                Type = AlexaErrorType.RateLimitExceeded,
            },
        };

        SkillRequest request = CreateRequest(session);

        // Act
        var actual = await ProcessRequestAsync(request);

        ResponseBody response = AssertResponse(actual);

        // Assert
        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");

        var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
        ssml.Ssml.ShouldBe("<speak>Goodbye.</speak>");
    }

    [xRetry.RetryFact]
    public async Task Alexa_Function_Can_Process_System_Exception_Request()
    {
        // Arrange
        var exception = new SystemExceptionRequest()
        {
            Error = new()
            {
                Message = "An unknown error occurred.",
                Type = AlexaErrorType.InternalServerError,
            },
            ErrorCause = new()
            {
                RequestId = "amzn1.echo-api.request.1234",
            },
        };

        SkillRequest request = CreateRequest(exception);

        // Act
        var actual = await ProcessRequestAsync(request);

        ResponseBody response = AssertResponse(actual);

        // Assert
        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");

        var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
        ssml.Ssml.ShouldBe("<speak>Sorry, something went wrong.</speak>");
    }

    private async Task<SkillResponse> ProcessRequestAsync(SkillRequest request)
    {
        // Arrange
        string json = JsonSerializer.Serialize(request, AppJsonSerializerContext.Default.SkillRequest);

        void Configure(IServiceCollection services)
        {
            services.AddLogging((builder) => builder.AddXUnit(this));
        }

        using var server = new LambdaTestServer(Configure);
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        await server.StartAsync(cancellationTokenSource.Token);

        LambdaTestContext context = await server.EnqueueAsync(json);

        // Queue a task to stop the Lambda function as soon as the response is processed
        _ = Task.Run(async () =>
        {
            await context.Response.WaitToReadAsync(cancellationTokenSource.Token);

            if (!cancellationTokenSource.IsCancellationRequested)
            {
                await cancellationTokenSource.CancelAsync();
            }
        });

        using var httpClient = server.CreateClient();

        // Act
        await FunctionEntrypoint.RunAsync<TestSettingsAlexaFunction>(httpClient, cancellationTokenSource.Token);

        // Assert
        context.Response.TryRead(out var result).ShouldBeTrue();

        result.ShouldNotBeNull();
        result.IsSuccessful.ShouldBeTrue();
        result.Duration.ShouldBeInRange(TimeSpan.FromTicks(1), TimeSpan.FromSeconds(2));
        result.Content.ShouldNotBeEmpty();

        json = await result.ReadAsStringAsync();
        var actual = JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.SkillResponse);

        actual.ShouldNotBeNull();

        return actual;
    }
}
