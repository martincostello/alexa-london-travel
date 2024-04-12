// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using MartinCostello.LondonTravel.Skill.Models;
using MartinCostello.Testing.AwsLambdaTestServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill;

[TestClass]
public class EndToEndTests : FunctionTests
{
    [TestMethod]
    public async Task Alexa_Function_Can_Process_Intent_Request()
    {
        // Arrange
        var intent = new IntentRequest()
        {
            Intent = new() { Name = "AMAZON.CancelIntent" },
        };

        var request = CreateRequest(intent);

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        var response = AssertResponse(actual);

        Assert.IsNull(response.Card);
        Assert.IsNull(response.OutputSpeech);
        Assert.IsNull(response.Reprompt);
    }

    [TestMethod]
    public async Task Alexa_Function_Can_Process_Launch_Request()
    {
        // Arrange
        var request = CreateRequest<LaunchRequest>();

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        var response = AssertResponse(actual, shouldEndSession: false);

        Assert.IsNull(response.Card);
        Assert.IsNull(response.Reprompt);

        Assert.IsNotNull(response.OutputSpeech);
        Assert.AreEqual("SSML", response.OutputSpeech.Type);
        Assert.AreEqual("<speak>Welcome to London Travel. You can ask me about disruption or for the status of any tube line, London Overground, the D.L.R. or the Elizabeth line.</speak>", response.OutputSpeech.Ssml);
    }

    [TestMethod]
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

        var request = CreateRequest(session);

        // Act
        var actual = await ProcessRequestAsync(request);

        var response = AssertResponse(actual);

        // Assert
        Assert.IsNull(response.Card);
        Assert.IsNull(response.Reprompt);

        Assert.IsNotNull(response.OutputSpeech);
        Assert.AreEqual("SSML", response.OutputSpeech.Type);
        Assert.AreEqual("<speak>Goodbye.</speak>", response.OutputSpeech.Ssml);
    }

    [TestMethod]
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

        var request = CreateRequest(exception);

        // Act
        var actual = await ProcessRequestAsync(request);

        var response = AssertResponse(actual);

        // Assert
        Assert.IsNull(response.Card);
        Assert.IsNull(response.Reprompt);

        Assert.IsNotNull(response.OutputSpeech);
        Assert.AreEqual("SSML", response.OutputSpeech.Type);
        Assert.AreEqual("<speak>Sorry, something went wrong.</speak>", response.OutputSpeech.Ssml);
    }

    private static async Task<SkillResponse> ProcessRequestAsync(SkillRequest request)
    {
        // Arrange
        string json = JsonSerializer.Serialize(request, AppJsonSerializerContext.Default.SkillRequest);

        static void Configure(IServiceCollection services)
        {
            services.AddLogging((builder) => builder.AddConsole());
        }

        using var server = new LambdaTestServer(Configure);
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        await server.StartAsync(cancellationTokenSource.Token);

        var context = await server.EnqueueAsync(json);

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
        Assert.IsTrue(context.Response.TryRead(out var result));

        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccessful);
        Assert.IsTrue(result.Duration > TimeSpan.Zero);
        Assert.IsTrue(result.Duration <= TimeSpan.FromSeconds(2));
        Assert.IsNotNull(result.Content);
        Assert.AreNotEqual(0, result.Content.Length);

        json = await result.ReadAsStringAsync();
        var actual = JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.SkillResponse);

        Assert.IsNotNull(actual);

        return actual;
    }
}
