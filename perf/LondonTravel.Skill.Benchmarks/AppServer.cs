// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Testing.AwsLambdaTestServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill.Benchmarks;

internal sealed class AppServer(string httpServerUrl) : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly LondonTravelSkill? _app = new();
    private bool _disposed;

    public async Task<byte[]> ProcessAsync(byte[] content)
    {
        var context = await _app!.EnqueueAsync(content);
        var result = await context.Response.ReadAsync(_cts.Token);
        return result.Content;
    }

    public async Task StartAsync()
    {
        if (_app is { } app)
        {
            await app.StartAsync(_cts.Token);

            Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_SECRETS_MANAGER", httpServerUrl);
            Environment.SetEnvironmentVariable("Skill__SkillApiUrl", httpServerUrl);
            Environment.SetEnvironmentVariable("Skill__TflApiUrl", httpServerUrl);

            _ = Task.Run(async () =>
            {
                using var client = app.CreateClient();
                await FunctionEntrypoint.RunAsync<TestAlexaFunction>(client, _cts.Token);
            });
        }
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (!_disposed)
        {
            await _cts.CancelAsync();
            _cts.Dispose();

            _app?.Dispose();
        }

        _disposed = true;
    }

    private sealed class LondonTravelSkill : LambdaTestServer
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddLogging((builder) => builder.ClearProviders().SetMinimumLevel(LogLevel.Warning));
        }
    }

    private sealed class TestAlexaFunction : AlexaFunction
    {
        protected override void Configure(ConfigurationBuilder builder)
        {
            base.Configure(builder);

            var config = new Dictionary<string, string?>()
            {
                ["AWS_ACCESS_KEY_ID"] = "aws-access-key-id",
                ["AWS_SECRET_ACCESS_KEY"] = "aws-secret-access-key",
                ["Logging:LogLevel:Default"] = "Critical",
                ["Skill:SkillId"] = "amzn1.ask.skill.49f13574-8134-4748-afeb-62ef1defffa6",
                ["Skill:TflApplicationId"] = "tfl-application-id",
                ["Skill:TflApplicationKey"] = "tfl-application-key",
                ["Skill:VerifySkillId"] = "true",
            };

            builder.AddEnvironmentVariables()
                   .AddInMemoryCollection(config);
        }
    }
}
