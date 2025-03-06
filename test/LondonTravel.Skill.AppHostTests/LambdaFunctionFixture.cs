// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Amazon.Lambda;
using Amazon.Runtime;
using MartinCostello.Logging.XUnit;
using MartinCostello.LondonTravel.Skill.AppHost;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill.AppHostTests;

public sealed class LambdaFunctionFixture : IAsyncLifetime, IDisposable, ITestOutputHelperAccessor
{
    private LambdaFunctionApplication? _application;
    private bool _disposed;
    private HttpServer? _httpServer;
    private string? _serviceUrl;

    ~LambdaFunctionFixture()
        => Dispose(false);

    public ITestOutputHelper? OutputHelper { get; set; }

    public AmazonLambdaClient CreateClient()
    {
        ThrowIfDisposed();
        ThrowIfNotStarted();

        var credentials = new AnonymousAWSCredentials();
        var clientConfig = new AmazonLambdaConfig() { ServiceURL = _serviceUrl };

        return new AmazonLambdaClient(credentials, clientConfig);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_application is { })
        {
            await _application.DisposeAsync();
        }

        if (_httpServer is { })
        {
            await _httpServer.DisposeAsync();
        }

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        if (_application != null)
        {
            throw new InvalidOperationException("The Lambda function has already been started.");
        }

        using var timeout = new CancellationTokenSource(Debugger.IsAttached ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(30));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);

        _httpServer = new HttpServer(ConfigureServices, AddHttpServerEndpoints);
        await _httpServer.StartAsync(cts.Token);

        _application = new LambdaFunctionApplication(_httpServer.ServerUrl.ToString(), ConfigureServices);
        await _application.StartAsync(cts.Token);

        using var client = _application.CreateHttpClient(ResourceNames.LambdaEmulator);
        _serviceUrl = client.BaseAddress!.ToString();
    }

    async ValueTask IAsyncLifetime.InitializeAsync()
        => await InitializeAsync(TestContext.Current.CancellationToken);

    private void ConfigureServices(IServiceCollection services)
        => services.AddLogging((p) => p.AddXUnit(this).SetMinimumLevel(LogLevel.Warning));

    private void AddHttpServerEndpoints(IEndpointRouteBuilder builder)
    {
        SecretsManager.AddEndpoints(builder);
        TflApi.AddEndpoints(builder);
        UserPreferences.AddEndpoints(builder);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _httpServer?.Dispose();
                _application?.Dispose();
            }

            _disposed = true;
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(_httpServer))]
    private void ThrowIfNotStarted()
    {
        if (_httpServer is null)
        {
            throw new InvalidOperationException("The Lambda function has not been started.");
        }
    }
}
