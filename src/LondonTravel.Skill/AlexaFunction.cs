// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using MartinCostello.LondonTravel.Skill.Extensions;
using MartinCostello.LondonTravel.Skill.Intents;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class representing the AWS Lambda function entry-point for the London Travel Amazon Alexa skill.
/// </summary>
[LambdaStartup]
public class AlexaFunction : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Whether the instance has been disposed.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The <see cref="ServiceProvider"/> to use.
    /// </summary>
    private ServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlexaFunction"/> class.
    /// </summary>
    public AlexaFunction()
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="AlexaFunction"/> class.
    /// </summary>
    ~AlexaFunction()
    {
        Dispose(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async virtual ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_serviceProvider is not null)
            {
                await _serviceProvider.DisposeAsync();
            }

            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Handles a request to the skill as an asynchronous operation.
    /// </summary>
    /// <param name="request">The skill request.</param>
    /// <param name="context">The AWS Lambda execution context.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the skill's response.
    /// </returns>
    [LambdaFunction]
    public async Task<SkillResponse> HandlerAsync(SkillRequest request, ILambdaContext context)
    {
        context.Logger.LogLine($"Invoking skill request of type {request.Request.GetType().Name}.");

        var handler = _serviceProvider.GetRequiredService<FunctionHandler>();

        return await handler.HandleAsync(request);
    }

    /// <summary>
    /// Initializes the skill as an asynchronous operation.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to initialize the skill.
    /// </returns>
    public Task<bool> InitializeAsync()
    {
        _serviceProvider ??= CreateServiceProvider();
        return Task.FromResult(true);
    }

    /// <summary>
    /// Configures the <see cref="IServiceCollection"/> to use.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging((builder) =>
        {
            var options = new LambdaLoggerOptions()
            {
                Filter = FilterLogs,
            };

            builder.AddLambdaLogger(options);
        });

        services.AddHttpClients();
        services.AddPolly();

        services.TryAddSingleton((_) => SkillConfiguration.CreateDefaultConfiguration());

        services.AddSingleton<AlexaSkill>();
        services.AddSingleton<FunctionHandler>();
        services.AddSingleton<IntentFactory>();
        services.AddSingleton((_) => TelemetryConfiguration.CreateDefault());
        services.AddSingleton(CreateTelemetryClient);

        services.AddSingleton<EmptyIntent>();
        services.AddSingleton<HelpIntent>();
        services.AddSingleton<UnknownIntent>();

        services.AddTransient<CommuteIntent>();
        services.AddTransient<DisruptionIntent>();
        services.AddTransient<StatusIntent>();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _serviceProvider?.Dispose();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Creates an <see cref="TelemetryClient"/>.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
    /// <returns>
    /// The created instance of <see cref="TelemetryClient"/>.
    /// </returns>
    private static TelemetryClient CreateTelemetryClient(IServiceProvider serviceProvider)
    {
        var config = serviceProvider.GetRequiredService<SkillConfiguration>();
        var configuration = serviceProvider.GetRequiredService<TelemetryConfiguration>();

        return new TelemetryClient(configuration)
        {
            InstrumentationKey = config.ApplicationInsightsKey,
        };
    }

    /// <summary>
    /// Filters the Lambda logs.
    /// </summary>
    /// <param name="name">The name of the log.</param>
    /// <param name="level">The log level.</param>
    /// <returns>
    /// <see langword="true"/> to log the message; otherwise <see langword="false"/>.
    /// </returns>
    private static bool FilterLogs(string name, LogLevel level)
    {
        if (level < LogLevel.Warning &&
            (name.StartsWith("System.", StringComparison.Ordinal) ||
             name.StartsWith("Microsoft.", StringComparison.Ordinal)))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Creates the <see cref="ServiceProvider"/> to use.
    /// </summary>
    /// <returns>
    /// The <see cref="ServiceProvider"/> to use.
    /// </returns>
    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        ConfigureServices(services);

        return services.BuildServiceProvider();
    }
}
