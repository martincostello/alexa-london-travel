// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Testing.AwsLambdaTestServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill.Benchmarks;

internal sealed class AppServer : IDisposable
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
            _ = Task.Run(async () =>
            {
                using var client = app.CreateClient();
                await FunctionEntrypoint.RunAsync<TestAlexaFunction>(client, _cts.Token);
            });
        }
    }

    public async Task StopAsync() => await _cts.CancelAsync();

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (!_disposed)
        {
            _cts.Cancel();
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
                ["Logging:LogLevel:Default"] = "Critical",
                ["Skill:SkillApiUrl"] = "https://londontravel.martincostello.local/",
                ["Skill:SkillId"] = "amzn1.ask.skill.49f13574-8134-4748-afeb-62ef1defffa6",
                ["Skill:TflApiUrl"] = "https://api.tfl.gov.uk/",
                ["Skill:TflApplicationId"] = "my-tfl-app-id",
                ["Skill:TflApplicationKey"] = "my-tfl-app-key",
                ["Skill:VerifySkillId"] = "true",
            };

            builder.AddInMemoryCollection(config);
        }
    }
}
