// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using JustEat.HttpClientInterception;
using MartinCostello.LondonTravel.Skill.Models;
using MartinCostello.Testing.AwsLambdaTestServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill;

public class EndToEndTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    private const int TimeoutMilliseconds = 15_000;

    [Theory(Timeout = TimeoutMilliseconds)]
    [InlineData("AMAZON.CancelIntent")]
    [InlineData("AMAZON.StopIntent")]
    public async Task Alexa_Function_Can_Process_Intent_Request(string name)
    {
        // Arrange
        var request = CreateIntentRequest(name);

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        var response = AssertResponse(actual);

        response.Card.ShouldBeNull();
        response.OutputSpeech.ShouldBeNull();
        response.Reprompt.ShouldBeNull();
    }

    [Fact(Timeout = TimeoutMilliseconds)]
    public async Task Alexa_Function_Can_Process_Intent_Request_For_Help()
    {
        // Arrange
        var request = CreateIntentRequest("AMAZON.HelpIntent");

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        var response = AssertResponse(actual, shouldEndSession: false);

        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
        response.OutputSpeech.Ssml.ShouldBe("<speak><p>This skill allows you to check for the status of a specific line, or for disruption in general. You can ask about any London Underground line, London Overground, the Docklands Light Railway or the Elizabeth line.</p><p>Asking about disruption in general provides information about any lines that are currently experiencing issues, such as any delays or planned closures.</p><p>Asking for the status for a specific line provides a summary of the current service, such as whether there is a good service or if there are any delays.</p><p>If you link your account and setup your preferences in the London Travel website, you can ask about your commute to quickly find out the status of the lines you frequently use.</p><p>What would you like to do?</p></speak>");
    }

    [Fact(Timeout = TimeoutMilliseconds)]
    public async Task Alexa_Function_Can_Process_Intent_Request_With_Unknown_Intent()
    {
        // Arrange
        var request = CreateIntentRequest("FooIntent");

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        var response = AssertResponse(actual, shouldEndSession: true);

        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
        response.OutputSpeech.Ssml.ShouldBe("<speak>Sorry, I don't understand how to do that.</speak>");
    }

    [Fact(Timeout = TimeoutMilliseconds)]
    public async Task Alexa_Function_Can_Process_Intent_Request_For_Disruption()
    {
        // Arrange
        var request = CreateIntentRequest("DisruptionIntent");

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        var response = AssertResponse(actual, shouldEndSession: true);

        response.Card.ShouldNotBeNull();
        response.Card.ShouldBeOfType<StandardCard>();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
        response.OutputSpeech.Ssml.ShouldBe("<speak>There is currently no disruption on the tube, London Overground, the D.L.R. or the Elizabeth line.</speak>");
    }

    [Theory(Timeout = TimeoutMilliseconds)]
    [InlineData("Northern", "There is a good service on the Northern line.")]
    [InlineData("Windrush", "There is a good service on the Windrush line.")]
    public async Task Alexa_Function_Can_Process_Intent_Request_For_Line_Status(
        string line,
        string expected)
    {
        // Arrange
        var request = CreateIntentRequest(
            "StatusIntent",
            new Slot() { Name = "LINE", Value = line });

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        var response = AssertResponse(actual, shouldEndSession: true);

        response.Card.ShouldNotBeNull();
        response.Card.ShouldBeOfType<StandardCard>();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
        response.OutputSpeech.Ssml.ShouldBe($"<speak>{expected}</speak>");
    }

    [Fact(Timeout = TimeoutMilliseconds)]
    public async Task Alexa_Function_Can_Process_Launch_Request()
    {
        // Arrange
        var request = CreateRequest<LaunchRequest>();

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        var response = AssertResponse(actual, shouldEndSession: false);

        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
        response.OutputSpeech.Ssml.ShouldBe("<speak>Welcome to London Travel. You can ask me about disruption or for the status of any tube line, London Overground, the D.L.R. or the Elizabeth line.</speak>");
    }

    [Fact(Timeout = TimeoutMilliseconds)]
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
        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
        response.OutputSpeech.Ssml.ShouldBe("<speak>Goodbye.</speak>");
    }

    [Fact(Timeout = TimeoutMilliseconds)]
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
        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
        response.OutputSpeech.Ssml.ShouldBe("<speak>Sorry, something went wrong.</speak>");
    }

    private async Task<SkillResponse> ProcessRequestAsync(SkillRequest request)
    {
        // Arrange
        string json = JsonSerializer.Serialize(request, AppJsonSerializerContext.Default.SkillRequest);

        using var server = new LambdaTestServer((services) => services.AddLogging((builder) => builder.AddXUnit(OutputHelper)));
        using var cancellationTokenSource = new CancellationTokenSource();

        await server.StartAsync(cancellationTokenSource.Token);

        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));

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
        try
        {
            await FunctionEntrypoint.RunAsync<TestAlexaFunctionWithHttpRequests>(httpClient, cancellationTokenSource.Token);
        }
        catch (UriFormatException)
        {
            // Ignore exception thrown when AWS_LAMBDA_RUNTIME_API is cleared
        }

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

    private sealed class TestAlexaFunctionWithHttpRequests() : TestSettingsAlexaFunction(TestContext.Current.TestOutputHelper!)
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpMessageHandlerBuilderFilter, HttpRequestInterceptionFilter>(
                (_) =>
                {
                    var options = new HttpClientInterceptorOptions()
                        .ThrowsOnMissingRegistration()
                        .RegisterBundleFromResourceStream<EndToEndTests>("tfl-no-disruptions.json")
                        .RegisterBundleFromResourceStream<EndToEndTests>("tfl-line-statuses.json");

                    return new HttpRequestInterceptionFilter(options);
                });

            base.ConfigureServices(services);
        }
    }
}
