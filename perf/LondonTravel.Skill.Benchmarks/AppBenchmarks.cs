// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill.Benchmarks;

[EventPipeProfiler(EventPipeProfile.CpuSampling)]
[MemoryDiagnoser]
public class AppBenchmarks : IAsyncDisposable
{
    private static readonly Dictionary<string, byte[]> _payloads = GetPayloads();
    private HttpServer? _proxy;
    private AppServer? _app;
    private bool _disposed;

    [GlobalSetup]
    public async Task StartServer()
    {
        _proxy = new((services) => services.AddLogging((builder) => builder.SetMinimumLevel(LogLevel.Warning)));

        if (_proxy is { } proxy)
        {
            await proxy.StartAsync();
        }

        _app = new(_proxy.ServerUrl);

        if (_app is { } app)
        {
            await app.StartAsync();
        }
    }

    [GlobalCleanup]
    public async Task StopServer()
    {
        if (_app is { } app)
        {
            await app.DisposeAsync();
            _app = null;
        }

        if (_proxy is { } proxy)
        {
            await proxy.DisposeAsync();
            _proxy = null;
        }
    }

    [Benchmark]
    public async Task<byte[]> Cancel()
        => await _app!.ProcessAsync(_payloads[nameof(Cancel)]);

    [Benchmark]
    public async Task<byte[]> Commute()
        => await _app!.ProcessAsync(_payloads[nameof(Commute)]);

    [Benchmark]
    public async Task<byte[]> Disruption()
        => await _app!.ProcessAsync(_payloads[nameof(Disruption)]);

    [Benchmark]
    public async Task<byte[]> Help()
        => await _app!.ProcessAsync(_payloads[nameof(Help)]);

    [Benchmark]
    public async Task<byte[]> Launch()
        => await _app!.ProcessAsync(_payloads[nameof(Launch)]);

    [Benchmark]
    public async Task<byte[]> SessionEnded()
        => await _app!.ProcessAsync(_payloads[nameof(SessionEnded)]);

    [Benchmark]
    public async Task<byte[]> Status()
        => await _app!.ProcessAsync(_payloads[nameof(Status)]);

    [Benchmark]
    public async Task<byte[]> Stop()
        => await _app!.ProcessAsync(_payloads[nameof(Stop)]);

    [Benchmark]
    public async Task<byte[]> UnknownIntent()
        => await _app!.ProcessAsync(_payloads[nameof(UnknownIntent)]);

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_app is not null)
            {
                await _app.DisposeAsync();
                _app = null;
            }

            if (_proxy is not null)
            {
                await _proxy.DisposeAsync();
                _proxy = null;
            }
        }

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    private static Dictionary<string, byte[]> GetPayloads()
    {
        var assembly = typeof(AppBenchmarks).Assembly;
        var payloads = new Dictionary<string, byte[]>();

        foreach (string name in assembly.GetManifestResourceNames())
        {
            using var resource = assembly.GetManifestResourceStream(name);
            using var stream = new MemoryStream();

            resource!.CopyTo(stream);

            string key = Path.GetFileNameWithoutExtension(name).Split('.')[^1];
            payloads[key] = stream.ToArray();
        }

        return payloads;
    }
}
