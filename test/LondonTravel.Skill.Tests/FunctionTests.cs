// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using JustEat.HttpClientInterception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Shouldly;
using Xunit.Abstractions;

namespace MartinCostello.LondonTravel.Skill
{
    public abstract class FunctionTests
    {
        protected FunctionTests(ITestOutputHelper outputHelper)
        {
            Logger = new XunitLambdaLogger(outputHelper);
            Interceptor = new HttpClientInterceptorOptions().ThrowsOnMissingRegistration();
        }

        protected HttpClientInterceptorOptions Interceptor { get; }

        protected ILambdaLogger Logger { get; }

        protected virtual ResponseBody AssertResponse(SkillResponse actual, bool? shouldEndSession = true)
        {
            actual.ShouldNotBeNull();
            actual.Version.ShouldBe("1.0");
            actual.Response.ShouldNotBeNull();
            actual.Response.ShouldEndSession.ShouldBe(shouldEndSession);

            return actual.Response;
        }

        protected virtual ILambdaContext CreateContext()
        {
            return new TestLambdaContext()
            {
                Logger = Logger,
            };
        }

        protected virtual AlexaFunction CreateFunction()
        {
            var config = SkillConfiguration.CreateDefaultConfiguration();

            config.ApplicationInsightsKey = "my-application-insights-key";
            config.SkillApiUrl = "https://londontravel.martincostello.local/";
            config.SkillId = "my-skill-id";
            config.TflApplicationId = "my-tfl-app-id";
            config.TflApplicationKey = "my-tfl-app-key";
            config.VerifySkillId = true;

            return new TestAlexaFunction(config, Interceptor);
        }

        protected virtual SkillRequest CreateIntentRequest(string name)
        {
            var request = new IntentRequest()
            {
                Intent = new Intent()
                {
                    Name = name,
                },
            };

            return CreateRequest(request);
        }

        protected virtual SkillRequest CreateRequest<T>(T request = null)
            where T : Request, new()
        {
            return new SkillRequest()
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
        }

        private sealed class TestAlexaFunction : AlexaFunction
        {
            public TestAlexaFunction(SkillConfiguration config, HttpClientInterceptorOptions options)
            {
                Config = config;
                Options = options;
            }

            private SkillConfiguration Config { get; }

            private HttpClientInterceptorOptions Options { get; }

            protected override void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton(Config);
                services.AddSingleton<IHttpMessageHandlerBuilderFilter, HttpRequestInterceptionFilter>(
                    (_) => new HttpRequestInterceptionFilter(Options));
            }
        }
    }
}
