// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Amazon.Lambda;
using Amazon.Runtime;
using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill.AppHostTests;

public sealed class LambdaFunctionFixture : IAsyncLifetime, ITestOutputHelperAccessor
{
    private LambdaFunctionApplication? _application;
    private bool _disposed;
    private HttpServer? _httpServer;

    public ITestOutputHelper? OutputHelper { get; set; }

    public AmazonLambdaClient CreateClient()
    {
        ThrowIfDisposed();
        ThrowIfNotStarted();

        var credentials = new AnonymousAWSCredentials();
        var clientConfig = new AmazonLambdaConfig() { ServiceURL = _application?.ServiceUrl };

        return new AmazonLambdaClient(credentials, clientConfig);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_application is { })
            {
                await _application.DisposeAsync();
            }

            if (_httpServer is { })
            {
                await _httpServer.DisposeAsync();
            }

            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        if (_application != null)
        {
            throw new InvalidOperationException("The Lambda function has already been started.");
        }

        _httpServer = new(ConfigureServices);
        await _httpServer.StartAsync(cancellationToken);

        _application = new(_httpServer.ServerUrl, ConfigureServices);
        await _application.StartAsync(cancellationToken);
    }

    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        using var timeout = new CancellationTokenSource(Debugger.IsAttached ? Timeout.InfiniteTimeSpan : TimeSpan.FromMinutes(1));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, TestContext.Current.CancellationToken);

        await InitializeAsync(cts.Token);
    }

    private void ConfigureServices(IServiceCollection services)
        => services.AddLogging((builder) =>
        {
            builder.ClearProviders()
                   .AddXUnit(this)
                   .SetMinimumLevel(LogLevel.Warning);
        });

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(_httpServer))]
    private void ThrowIfNotStarted()
    {
        if (_httpServer is null)
        {
            throw new InvalidOperationException("The HTTP server has not been started.");
        }

        if (_application is null)
        {
            throw new InvalidOperationException("The Lambda function has not been started.");
        }
    }
}
