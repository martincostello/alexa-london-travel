// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using Amazon.Lambda.Core;
using MartinCostello.LondonTravel.Skill.Extensions;
using MartinCostello.LondonTravel.Skill.Intents;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.LondonTravel.Skill
{
    /// <summary>
    /// A class for resolving the service's dependencies. This class cannot be inherited.
    /// </summary>
    internal static class ServiceResolver
    {
        /// <summary>
        /// Gets the service provider to use for the current AWS Lambda context.
        /// </summary>
        /// <param name="context">The current AWS Lambda context.</param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> to use.
        /// </returns>
        public static IServiceCollection GetServiceCollection(ILambdaContext context)
        {
            var services = new ServiceCollection();

            services.AddHttpClients();
            services.AddPolly();

            services.AddSingleton(context);
            services.AddSingleton((_) => SkillConfiguration.CreateDefaultConfiguration());

            services.AddSingleton<AlexaSkill>();
            services.AddSingleton<IntentFactory>();
            services.AddSingleton((_) => TelemetryConfiguration.CreateDefault());
            services.AddSingleton(CreateTelemetryClient);

            services.AddSingleton<EmptyIntent>();
            services.AddSingleton<HelpIntent>();

            services.AddTransient<CommuteIntent>();
            services.AddTransient<DisruptionIntent>();
            services.AddTransient<StatusIntent>();
            services.AddTransient<UnknownIntent>();

            return services;
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
    }
}
