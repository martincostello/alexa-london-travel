// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using MartinCostello.LondonTravel.Skill.AppHost;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.LondonTravel.Skill.AppHostTests;

internal sealed class LambdaFunctionApplication(
    string httpServerUrl,
    Action<IServiceCollection> configureServices) : IAsyncDisposable
{
    private readonly CancellationTokenSource _onDisposed = new();

    private DistributedApplication? _application;
    private Uri? _baseAddress;
    private bool _disposed;
    private bool _isStarted;
    private CancellationTokenSource? _onStopped;

    public string ServiceUrl
    {
        get
        {
            ThrowIfDisposed();
            ThrowIfNotStarted();

            return _baseAddress.ToString();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_application is { })
            {
                await _application.StopAsync(TestContext.Current.CancellationToken);
            }

            _isStarted = false;

            if (_onDisposed != null)
            {
                if (!_onDisposed.IsCancellationRequested)
                {
                    await _onDisposed.CancelAsync();
                }

                _onDisposed.Dispose();
                _onStopped?.Dispose();
            }

            _application?.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_isStarted)
        {
            throw new InvalidOperationException("The application has already been started.");
        }

        _onStopped = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _onDisposed.Token);

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.LondonTravel_Skill_AppHost>(cancellationToken);

        builder.CreateResourceBuilder<ProjectResource>(ResourceNames.LambdaFunction)
               .WithEnvironment("AWS_ACCESS_KEY_ID", "aws-access-key-id")
               .WithEnvironment("AWS_SECRET_ACCESS_KEY", "aws-secret-access-key")
               .WithEnvironment("AWS_ENDPOINT_URL_SECRETS_MANAGER", httpServerUrl)
               .WithEnvironment("Skill__SkillApiUrl", httpServerUrl)
               .WithEnvironment("Skill__TflApiUrl", httpServerUrl);

        configureServices?.Invoke(builder.Services);

        _application = await builder.BuildAsync(cancellationToken);
        await _application.StartAsync(cancellationToken);

        using (var client = _application.CreateHttpClient(ResourceNames.LambdaEmulator))
        {
            _baseAddress = client.BaseAddress;
        }

        _isStarted = true;
    }

    private void ThrowIfDisposed()
        => ObjectDisposedException.ThrowIf(_disposed, this);

    [MemberNotNull(nameof(_baseAddress))]
    [MemberNotNull(nameof(_application))]
    private void ThrowIfNotStarted()
    {
        if (_application is null)
        {
            throw new InvalidOperationException("The application has not been started.");
        }

        if (_baseAddress is null)
        {
            throw new InvalidOperationException("No server address for the application is available.");
        }
    }
}
