// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace MartinCostello.LondonTravel.Skill.Benchmarks;

[EventPipeProfiler(EventPipeProfile.CpuSampling)]
[MemoryDiagnoser]
public class AppBenchmarks : IDisposable
{
    private readonly Dictionary<string, byte[]> _payloads = GetPayloads();
    private AppServer? _app = new();
    private bool _disposed;

    [GlobalSetup]
    public async Task StartServer()
    {
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
            await app.StopAsync();
            _app = null;
        }
    }

    [Benchmark]
    public async Task<byte[]> Cancel()
        => await _app!.ProcessAsync(_payloads[nameof(Cancel)]);

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
    public async Task<byte[]> Stop()
        => await _app!.ProcessAsync(_payloads[nameof(Stop)]);

    [Benchmark]
    public async Task<byte[]> UnknownIntent()
        => await _app!.ProcessAsync(_payloads[nameof(UnknownIntent)]);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _app?.Dispose();
            _app = null;
        }

        _disposed = true;
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
