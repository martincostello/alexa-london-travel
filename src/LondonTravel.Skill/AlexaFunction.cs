// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Skill.Extensions;
using MartinCostello.LondonTravel.Skill.Intents;
using MartinCostello.LondonTravel.Skill.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class representing the AWS Lambda function entry-point for the London Travel Amazon Alexa skill.
/// </summary>
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
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the skill's response.
    /// </returns>
    public async Task<SkillResponse> HandlerAsync(SkillRequest request)
    {
        var handler = _serviceProvider.GetRequiredService<FunctionHandler>();
        var logger = _serviceProvider.GetRequiredService<ILogger<AlexaFunction>>();

        Log.InvokingSkillRequest(logger, request.Request.Type);

        var converted = request.FromModel();

        var response = await handler.HandleAsync(converted);

        return response.ToModel();
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
    /// Configures the <see cref="ConfigurationBuilder"/> to use.
    /// </summary>
    /// <param name="builder">The configuration builder to configure.</param>
    protected virtual void Configure(ConfigurationBuilder builder)
    {
        builder.AddJsonFile("appsettings.json", optional: true)
               .AddEnvironmentVariables();
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
        });

        services.AddHttpClients();

        services.AddSingleton<AlexaSkill>();
        services.AddSingleton<FunctionHandler>();
        services.AddSingleton<IntentFactory>();
        services.AddSingleton<IValidateOptions<SkillConfiguration>, ValidateSkillConfiguration>();
        services.AddSingleton((p) => p.GetRequiredService<IOptions<SkillConfiguration>>().Value);
        services.AddSingleton(configuration);

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
