// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using JustEat.HttpClientInterception;
using MartinCostello.LondonTravel.Skill.Models;
using MartinCostello.Testing.AwsLambdaTestServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

[assembly: DoNotParallelize]

namespace MartinCostello.LondonTravel.Skill;

[TestClass]
public sealed class EndToEndTests
{
    private const int TimeoutMilliseconds = 20_000;

    private static readonly ConsoleLoggerProvider ConsoleLogger = new(new ConsoleLoggerOptionsMonitor());

    public TestContext TestContext { get; set; }

    private HttpClientInterceptorOptions Interceptor { get; } = new HttpClientInterceptorOptions().ThrowsOnMissingRegistration();

    [TestMethod]
    [Timeout(TimeoutMilliseconds, CooperativeCancellation = true)]
    [DataRow("AMAZON.CancelIntent")]
    [DataRow("AMAZON.StopIntent")]
    public async Task Alexa_Function_Can_Process_Intent_Request(string name)
    {
        // Arrange
        var request = CreateIntentRequest(name);

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        var response = AssertResponse(actual);

        Assert.IsNull(response.Card);
        Assert.IsNull(response.OutputSpeech);
        Assert.IsNull(response.Reprompt);
    }

    [TestMethod]
    [Timeout(TimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Alexa_Function_Can_Process_Intent_Request_For_Help()
    {
        // Arrange
        var request = CreateIntentRequest("AMAZON.HelpIntent");

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        var response = AssertResponse(actual, shouldEndSession: false);

        Assert.IsNull(response.Card);
        Assert.IsNull(response.Reprompt);

        Assert.IsNotNull(response.OutputSpeech);
        Assert.AreEqual("SSML", response.OutputSpeech.Type);
        Assert.AreEqual("<speak><p>This skill allows you to check for the status of a specific line, or for disruption in general. You can ask about any London Underground line, London Overground, the Docklands Light Railway or the Elizabeth line.</p><p>Asking about disruption in general provides information about any lines that are currently experiencing issues, such as any delays or planned closures.</p><p>Asking for the status for a specific line provides a summary of the current service, such as whether there is a good service or if there are any delays.</p><p>If you link your account and setup your preferences in the London Travel website, you can ask about your commute to quickly find out the status of the lines you frequently use.</p><p>What would you like to do?</p></speak>", response.OutputSpeech.Ssml);
    }

    [TestMethod]
    [Timeout(TimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Alexa_Function_Can_Process_Intent_Request_With_Unknown_Intent()
    {
        // Arrange
        var request = CreateIntentRequest("FooIntent");

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        var response = AssertResponse(actual, shouldEndSession: true);

        Assert.IsNull(response.Card);
        Assert.IsNull(response.Reprompt);

        Assert.IsNotNull(response.OutputSpeech);
        Assert.AreEqual("SSML", response.OutputSpeech.Type);
        Assert.AreEqual("<speak>Sorry, I don't understand how to do that.</speak>", response.OutputSpeech.Ssml);
    }

    [TestMethod]
    [Timeout(TimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Alexa_Function_Can_Process_Intent_Request_For_Disruption()
    {
        // Arrange
        var request = CreateIntentRequest("DisruptionIntent");

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        var response = AssertResponse(actual, shouldEndSession: true);

        Assert.IsNotNull(response.Card);
        Assert.IsInstanceOfType<StandardCard>(response.Card);
        Assert.IsNull(response.Reprompt);

        Assert.IsNotNull(response.OutputSpeech);
        Assert.AreEqual("SSML", response.OutputSpeech.Type);
        Assert.AreEqual("<speak>There is currently no disruption on the tube, London Overground, the D.L.R. or the Elizabeth line.</speak>", response.OutputSpeech.Ssml);
    }

    [TestMethod]
    [Timeout(TimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Alexa_Function_Can_Process_Intent_Request_For_Line_Status()
    {
        // Arrange
        var request = CreateIntentRequest(
            "StatusIntent",
            new Slot() { Name = "LINE", Value = "Northern" });

        // Act
        var actual = await ProcessRequestAsync(request);

        // Assert
        var response = AssertResponse(actual, shouldEndSession: true);

        Assert.IsNotNull(response.Card);
        Assert.IsInstanceOfType<StandardCard>(response.Card);
        Assert.IsNull(response.Reprompt);

        Assert.IsNotNull(response.OutputSpeech);
        Assert.AreEqual("SSML", response.OutputSpeech.Type);
        Assert.AreEqual("<speak>There is a good service on the Northern line.</speak>", response.OutputSpeech.Ssml);
    }

    [TestMethod]
    [Timeout(TimeoutMilliseconds, CooperativeCancellation = true)]
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
    [Timeout(TimeoutMilliseconds, CooperativeCancellation = true)]
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
    [Timeout(TimeoutMilliseconds, CooperativeCancellation = true)]
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

    private static SkillRequest CreateIntentRequest(string name, params Slot[] slots)
    {
        var request = new IntentRequest()
        {
            Intent = new Intent()
            {
                Name = name,
            },
        };

        if (slots.Length > 0)
        {
            request.Intent.Slots = [];

            foreach (var slot in slots)
            {
                request.Intent.Slots[slot.Name] = slot;
            }
        }

        return CreateRequest(request);
    }

    private static SkillRequest CreateRequest<T>(T? request = null)
        where T : Request, new()
    {
        var application = new Application()
        {
            ApplicationId = "my-skill-id",
        };

        var user = new User()
        {
            UserId = Guid.NewGuid().ToString(),
        };

        var result = new SkillRequest()
        {
            Context = new()
            {
                System = new()
                {
                    Application = application,
                    Device = new()
                    {
                        SupportedInterfaces = new()
                        {
                            ["AudioPlayer"] = new(),
                        },
                    },
                    User = user,
                },
            },
            Request = request ?? new T(),
            Session = new()
            {
                Application = application,
                Attributes = [],
                New = true,
                User = user,
            },
            Version = "1.0",
        };

        result.Request.Locale = "en-GB";

        return result;
    }

    private static ResponseBody AssertResponse(SkillResponse actual, bool? shouldEndSession = true)
    {
        Assert.IsNotNull(actual);
        Assert.AreEqual("1.0", actual.Version);
        Assert.IsNotNull(actual.Response);
        Assert.AreEqual(shouldEndSession, actual.Response.ShouldEndSession);

        return actual.Response;
    }

    private async Task<SkillResponse> ProcessRequestAsync(SkillRequest request)
    {
        // Arrange
        string json = JsonSerializer.Serialize(request, AppJsonSerializerContext.Default.SkillRequest);

        void Configure(IServiceCollection services)
        {
            services.AddLogging((builder) => builder.AddProvider(ConsoleLogger));
            services.AddSingleton<IHttpMessageHandlerBuilderFilter, HttpRequestInterceptionFilter>(
                (_) => new HttpRequestInterceptionFilter(Interceptor));
        }

        using var server = new LambdaTestServer(Configure);

        using var timeout = new CancellationTokenSource();
        timeout.CancelAfter(TimeSpan.FromSeconds(2));

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext?.CancellationTokenSource?.Token ?? default,
            timeout.Token);

        await server.StartAsync(linked.Token);

        var context = await server.EnqueueAsync(json);

        // Queue a task to stop the Lambda function as soon as the response is processed
        _ = Task.Run(
            async () =>
            {
                await context.Response.WaitToReadAsync(linked.Token);

                if (!linked.IsCancellationRequested)
                {
                    await linked.CancelAsync();
                }
            },
            linked.Token);

        using var httpClient = server.CreateClient();

        // Act
        await FunctionEntrypoint.RunAsync<TestAlexaFunction>(httpClient, linked.Token);

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

    private sealed class ConsoleLoggerOptionsMonitor : ConsoleLoggerOptions, IOptionsMonitor<ConsoleLoggerOptions>
    {
        public ConsoleLoggerOptions CurrentValue => this;

        public ConsoleLoggerOptions Get(string? name) => this;

        public IDisposable OnChange(Action<ConsoleLoggerOptions, string> listener) => null!;
    }

    private sealed class TestAlexaFunction : AlexaFunction
    {
        protected override void Configure(ConfigurationBuilder builder)
        {
            base.Configure(builder);

            var config = new Dictionary<string, string?>()
            {
                ["Logging:LogLevel:Default"] = "Warning",
                ["Skill:SkillApiUrl"] = "https://londontravel.martincostello.local/",
                ["Skill:SkillId"] = "my-skill-id",
                ["Skill:TflApiUrl"] = "https://api.tfl.gov.uk/",
                ["Skill:TflApplicationId"] = "my-tfl-app-id",
                ["Skill:TflApplicationKey"] = "my-tfl-app-key",
                ["Skill:VerifySkillId"] = "true",
            };

            builder.AddInMemoryCollection(config);
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpMessageHandlerBuilderFilter, HttpRequestInterceptionFilter>((_) =>
            {
                var options = new HttpClientInterceptorOptions()
                    .ThrowsOnMissingRegistration()
                    .RegisterBundleFromResourceStream<EndToEndTests>("tfl-no-disruptions.json")
                    .RegisterBundleFromResourceStream<EndToEndTests>("tfl-line-statuses.json");

                return new HttpRequestInterceptionFilter(options);
            });

            base.ConfigureServices(services);

            services.AddLogging((p) => p.ClearProviders().AddProvider(ConsoleLogger));
        }
    }
}
