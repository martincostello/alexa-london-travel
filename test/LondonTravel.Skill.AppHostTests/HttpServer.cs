// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill.AppHostTests;

internal sealed class HttpServer(
    Action<IServiceCollection> configureServices,
    Action<IEndpointRouteBuilder> configureEndpoints) : IAsyncLifetime, IDisposable
{
    private readonly CancellationTokenSource _onDisposed = new();

    private Uri? _baseAddress;
    private bool _disposed;
    private bool _isStarted;
    private IWebHost? _host;
    private CancellationTokenSource? _onStopped;

    ~HttpServer()
        => Dispose(false);

    public Uri ServerUrl
    {
        get
        {
            ThrowIfDisposed();
            ThrowIfNotStarted();

            return _baseAddress;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_host is { })
        {
            await _host.StopAsync(TestContext.Current.CancellationToken);
        }

        Dispose();
        GC.SuppressFinalize(this);
    }

    public HttpClient CreateClient()
    {
        ThrowIfDisposed();
        ThrowIfNotStarted();

        return new() { BaseAddress = _baseAddress };
    }

    public async ValueTask InitializeAsync()
        => await StartAsync(TestContext.Current.CancellationToken);

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_isStarted)
        {
            throw new InvalidOperationException("The proxy server has already been started.");
        }

        _onStopped = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _onDisposed.Token);

        var builder = new WebHostBuilder();
        builder.UseKestrel();

        ConfigureWebHost(builder);

        _host = builder.Build();

        await _host.StartAsync(_onStopped.Token);

        var serverAddresses = _host!.ServerFeatures.Get<IServerAddressesFeature>();
        string? serverUrl = serverAddresses?.Addresses?.FirstOrDefault();

        _baseAddress = serverUrl is null
            ? throw new InvalidOperationException("No server addresses are available.")
            : new Uri(serverUrl, UriKind.Absolute);

        _isStarted = true;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_onDisposed != null)
                {
                    if (!_onDisposed.IsCancellationRequested)
                    {
                        _onDisposed.Cancel();
                    }

                    _onDisposed.Dispose();
                    _onStopped?.Dispose();
                }

                _isStarted = false;
                _host?.Dispose();
            }

            _disposed = true;
        }
    }

    private void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(configureEndpoints);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddRouting();
        configureServices?.Invoke(services);
    }

    private void ConfigureWebHost(WebHostBuilder builder)
    {
        builder.UseContentRoot(Environment.CurrentDirectory);
        builder.UseShutdownTimeout(TimeSpan.Zero);

        builder.ConfigureServices(ConfigureServices);
        builder.Configure(Configure);
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(_baseAddress))]
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(_host))]
    private void ThrowIfNotStarted()
    {
        if (_host is null)
        {
            throw new InvalidOperationException("The proxy server has not been started.");
        }

        if (_baseAddress is null)
        {
            throw new InvalidOperationException("No server addresses are available.");
        }
    }
}
