// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using JustEat.HttpClientInterception;
using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill;

public abstract class FunctionTests : ITestOutputHelperAccessor
{
    protected FunctionTests(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        Interceptor = new HttpClientInterceptorOptions().ThrowsOnMissingRegistration();
    }

    public ITestOutputHelper OutputHelper { get; set; }

    protected HttpClientInterceptorOptions Interceptor { get; }

    protected virtual ResponseBody AssertResponse(SkillResponse actual, bool? shouldEndSession = true)
    {
        actual.ShouldNotBeNull();
        actual.Version.ShouldBe("1.0");
        actual.Response.ShouldNotBeNull();
        actual.Response.ShouldEndSession.ShouldBe(shouldEndSession);

        return actual.Response;
    }

    protected virtual SkillConfiguration CreateConfiguration()
    {
        var config = SkillConfiguration.CreateDefaultConfiguration();

        config.ApplicationInsightsKey = "my-application-insights-key";
        config.SkillApiUrl = "https://londontravel.martincostello.local/";
        config.SkillId = "my-skill-id";
        config.TflApplicationId = "my-tfl-app-id";
        config.TflApplicationKey = "my-tfl-app-key";
        config.VerifySkillId = true;

        return config;
    }

    protected virtual ILambdaContext CreateContext()
    {
        return new TestLambdaContext()
        {
            Logger = new XunitLambdaLogger(OutputHelper),
        };
    }

    protected virtual async Task<AlexaFunction> CreateFunctionAsync()
    {
        SkillConfiguration config = CreateConfiguration();
        var function = new TestAlexaFunction(config, Interceptor, OutputHelper);

        await function.InitializeAsync();

        return function;
    }

    protected virtual SkillRequest CreateIntentRequest(string name, params Slot[] slots)
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
            request.Intent.Slots = new Dictionary<string, Slot>();

            foreach (var slot in slots)
            {
                request.Intent.Slots[slot.Name] = slot;
            }
        }

        return CreateRequest(request);
    }

    protected virtual SkillRequest CreateRequest<T>(T request = null)
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
            Context = new Context()
            {
                AudioPlayer = new PlaybackState()
                {
                    PlayerActivity = "IDLE",
                },
                System = new AlexaSystem()
                {
                    Application = application,
                    Device = new Device()
                    {
                        SupportedInterfaces = new Dictionary<string, object>()
                        {
                            ["AudioPlayer"] = new object(),
                        },
                    },
                    User = user,
                },
            },
            Request = request ?? new T(),
            Session = new Session()
            {
                Application = application,
                Attributes = new Dictionary<string, object>(),
                New = true,
                User = user,
            },
            Version = "1.0",
        };

        result.Request.Locale = "en-GB";

        return result;
    }

    private sealed class TestAlexaFunction : AlexaFunction, ITestOutputHelperAccessor
    {
        public TestAlexaFunction(
            SkillConfiguration config,
            HttpClientInterceptorOptions options,
            ITestOutputHelper outputHelper)
        {
            Config = config;
            Options = options;
            OutputHelper = outputHelper;
        }

        public ITestOutputHelper OutputHelper { get; set; }

        private SkillConfiguration Config { get; }

        private HttpClientInterceptorOptions Options { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging((builder) => builder.AddXUnit(this));
            services.AddSingleton(Config);
            services.AddSingleton<IHttpMessageHandlerBuilderFilter, HttpRequestInterceptionFilter>(
                (_) => new HttpRequestInterceptionFilter(Options));

            base.ConfigureServices(services);
        }
    }
}
