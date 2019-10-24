// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
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
using Shouldly;
using Xunit.Abstractions;

namespace MartinCostello.LondonTravel.Skill
{
    public abstract class FunctionTests
    {
        protected FunctionTests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
            Interceptor = new HttpClientInterceptorOptions().ThrowsOnMissingRegistration();
        }

        protected HttpClientInterceptorOptions Interceptor { get; }

        private ITestOutputHelper OutputHelper { get; }

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

        protected virtual AlexaFunction CreateFunction()
        {
            SkillConfiguration config = CreateConfiguration();
            return new TestAlexaFunction(config, Interceptor, OutputHelper);
        }

        protected virtual SkillRequest CreateIntentRequest(string name, params Slot[] slots)
        {
            var intentSlots = new Dictionary<string, Slot>();

            if (slots?.Length > 0)
            {
                foreach (var slot in slots)
                {
                    intentSlots[slot.Name] = slot;
                }
            }

            var request = new IntentRequest()
            {
                Intent = new Intent()
                {
                    Name = name,
                    Slots = intentSlots,
                },
            };

            return CreateRequest(request);
        }

        protected virtual SkillRequest CreateRequest<T>(T request = null)
            where T : Request, new()
        {
            var result = new SkillRequest()
            {
                Request = request ?? new T(),
                Session = new Session()
                {
                    Application = new Application()
                    {
                        ApplicationId = "my-skill-id",
                    },
                },
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

            protected override void ConfigureServices(IServiceCollection services)
            {
                services.AddLogging((builder) => builder.AddXUnit(this));
                services.AddSingleton(Config);
                services.AddSingleton<IHttpMessageHandlerBuilderFilter, HttpRequestInterceptionFilter>(
                    (_) => new HttpRequestInterceptionFilter(Options));
            }
        }
    }
}
