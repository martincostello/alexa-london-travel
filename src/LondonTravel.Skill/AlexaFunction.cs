// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Amazon.Lambda.Core;
using MartinCostello.LondonTravel.Skill.Extensions;
using MartinCostello.LondonTravel.Skill.Intents;
using MartinCostello.LondonTravel.Skill.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class representing the AWS Lambda function entry-point for the London Travel Amazon Alexa skill.
/// </summary>
public class AlexaFunction : IAsyncDisposable, IDisposable
{
    private bool _disposed;
    private ServiceProvider? _serviceProvider;

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
    public virtual async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_serviceProvider is { })
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
    /// <param name="context">The Lambda request context.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the skill's response.
    /// </returns>
    public async Task<SkillResponse> HandlerAsync(SkillRequest request, ILambdaContext context)
    {
        EnsureInitialized();

        var meterProvider = _serviceProvider.GetRequiredService<MeterProvider>();

        var response = await OpenTelemetry.Instrumentation.AWSLambda.AWSLambdaWrapper.TraceAsync(
            _serviceProvider.GetRequiredService<OpenTelemetry.Trace.TracerProvider>(),
            HandlerCoreAsync,
            request,
            context);

        meterProvider.ForceFlush();

        return response;
    }

    /// <summary>
    /// Initializes the skill as an asynchronous operation.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to initialize the skill.
    /// </returns>
    [MemberNotNull(nameof(_serviceProvider))]
    public Task<bool> InitializeAsync()
    {
        _serviceProvider ??= CreateServiceProvider();
        return Task.FromResult(true);
    }

    internal static bool IsRunningInAwsLambda()
        => Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME") is { Length: > 0 } &&
           Environment.GetEnvironmentVariable("AWS_REGION") is { Length: > 0 };

    /// <summary>
    /// Configures the <see cref="ConfigurationBuilder"/> to use.
    /// </summary>
    /// <param name="builder">The configuration builder to configure.</param>
    protected virtual void Configure(ConfigurationBuilder builder)
    {
        builder.AddJsonFile("appsettings.json", optional: true)
               .AddEnvironmentVariables();

        if (IsRunningInAwsLambda())
        {
            builder.AddSecretsManager();
        }
    }

    /// <summary>
    /// Configures the <see cref="IServiceCollection"/> to use.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        var builder = new ConfigurationBuilder();

        Configure(builder);

        var configuration = builder.Build();

        services.AddOptions();
        services.Configure<SkillConfiguration>(configuration.GetSection("Skill"));

        services.AddLogging((builder) =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddJsonConsole();

            builder.AddOpenTelemetry((options) =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;

                options.SetResourceBuilder(SkillTelemetry.ResourceBuilder);
            });
        });

        services.AddHttpClients();
        services.AddTelemetry();

        services.AddSingleton<AlexaSkill>();
        services.AddSingleton<FunctionHandler>();
        services.AddSingleton<IntentFactory>();
        services.AddSingleton<IValidateOptions<SkillConfiguration>, ValidateSkillConfiguration>();
        services.AddSingleton((p) => p.GetRequiredService<IOptions<SkillConfiguration>>().Value);
        services.AddSingleton<IConfiguration>(configuration);

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

    private async Task<SkillResponse> HandlerCoreAsync(SkillRequest request, ILambdaContext context)
    {
        var handler = _serviceProvider!.GetRequiredService<FunctionHandler>();
        var logger = _serviceProvider!.GetRequiredService<ILogger<AlexaFunction>>();

        var metrics = _serviceProvider!.GetRequiredService<SkillMetrics>();
        metrics.SkillInvoked(request.Request.Type);

        using var activity = SkillTelemetry.ActivitySource.StartActivity("Skill Request");

        if (activity is { })
        {
            // TODO Remove when added to OpenTelemetry.Instrumentation.AWSLambda
            activity.SetTag("faas.instance", context.LogStreamName);
            activity.SetTag("faas.max_memory", context.MemoryLimitInMB * 1024 * 1024);

            activity.SetTag("alexa.request.id", request.Request.RequestId);
            activity.SetTag("alexa.request.locale", request.Request.Locale);
            activity.SetTag("alexa.request.type", request.Request.Type);

            if (request.Request is IntentRequest intent)
            {
                activity.SetTag("alexa.request.intent.name", intent.Intent?.Name);

                if (intent.Intent?.Slots is { Count: > 0 } slots)
                {
                    activity.SetTag("alexa.request.intent.slots", slots.Values.Select((p) => $"{p.Name}={p.Value}").ToArray());
                }
            }

            if (request.Context?.System is { } system)
            {
                activity.SetTag("alexa.context.system.application.id", system.Application?.ApplicationId);
                activity.SetTag("alexa.context.system.user.id", system.User?.UserId);
            }
        }

        Log.InvokingSkillRequest(logger, request.Request.Type);

        return await handler.HandleAsync(request);
    }

    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        ConfigureServices(services);

        return services.BuildServiceProvider();
    }

    [MemberNotNull(nameof(_serviceProvider))]
    private void EnsureInitialized()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException($"The function has not been initialized.");
        }
    }
}
