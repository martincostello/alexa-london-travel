// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using JustEat.HttpClientInterception;
using MartinCostello.LondonTravel.Skill.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill;

public abstract class FunctionTests(ITestOutputHelper outputHelper)
{
    public ITestOutputHelper OutputHelper { get; set; } = outputHelper;

    protected HttpClientInterceptorOptions Interceptor { get; } = new HttpClientInterceptorOptions().ThrowsOnMissingRegistration();

    protected virtual ResponseBody AssertResponse(SkillResponse actual, bool? shouldEndSession = true)
    {
        actual.ShouldNotBeNull();
        actual.Version.ShouldBe("1.0");
        actual.Response.ShouldNotBeNull();
        actual.Response.ShouldEndSession.ShouldBe(shouldEndSession);

        return actual.Response;
    }

    protected virtual async Task<AlexaFunction> CreateFunctionAsync()
    {
        var function = new TestAlexaFunction(Interceptor, OutputHelper);

        _ = await function.InitializeAsync();

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
            request.Intent.Slots = [];

            foreach (var slot in slots)
            {
                request.Intent.Slots[slot.Name] = slot;
            }
        }

        return CreateRequest(request);
    }

    protected virtual SkillRequest CreateRequest<T>(T? request = null)
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

    protected class TestSettingsAlexaFunction(ITestOutputHelper outputHelper) : AlexaFunction
    {
        protected override void Configure(ConfigurationBuilder builder)
        {
            base.Configure(builder);
            builder.AddJsonFile("testsettings.json");
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddLogging((p) => p.ClearProviders().AddXUnit(outputHelper));
        }
    }

    protected class TestAlexaFunction(
        HttpClientInterceptorOptions options,
        ITestOutputHelper outputHelper) : TestSettingsAlexaFunction(outputHelper)
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpMessageHandlerBuilderFilter, HttpRequestInterceptionFilter>(
                (_) => new HttpRequestInterceptionFilter(options));

            base.ConfigureServices(services);
        }
    }
}
