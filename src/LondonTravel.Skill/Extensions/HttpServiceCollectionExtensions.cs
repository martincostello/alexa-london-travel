// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Text.Json;
using MartinCostello.LondonTravel.Skill.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;

namespace MartinCostello.LondonTravel.Skill.Extensions
{
    /// <summary>
    /// A class containing HTTP-related extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
    /// </summary>
    internal static class HttpServiceCollectionExtensions
    {
        /// <summary>
        /// Adds HTTP clients to the services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>
        /// The value specified by <paramref name="services"/>.
        /// </returns>
        public static IServiceCollection AddHttpClients(this IServiceCollection services)
        {
            services
                .AddHttpClient(Options.DefaultName)
                .ApplyDefaultConfiguration();

            services
                .AddHttpClient(nameof(ISkillClient))
                .AddTypedClient(AddSkill)
                .ApplyDefaultConfiguration();

            services
                .AddHttpClient(nameof(ITflClient))
                .AddTypedClient(AddTfl)
                .ApplyDefaultConfiguration();

            services.AddSingleton(CreateJsonSerializerOptions);
            services.AddSingleton<IContentSerializer, SystemTextJsonContentSerializer>();
            services.AddTransient(CreateRefitSettings);

            return services;
        }

        /// <summary>
        /// Adds a typed client for the skill's API.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to configure the client with.</param>
        /// <param name="provider">The <see cref="IServiceProvider"/> to use.</param>
        /// <returns>
        /// The <see cref="ISkillClient"/> to use.
        /// </returns>
        private static ISkillClient AddSkill(HttpClient client, IServiceProvider provider)
        {
            var config = provider.GetRequiredService<SkillConfiguration>();
            var settings = provider.GetRequiredService<RefitSettings>();

            client.BaseAddress = new Uri(config.SkillApiUrl, UriKind.Absolute);

            return RestService.For<ISkillClient>(client, settings);
        }

        /// <summary>
        /// Adds a typed client for the TfL API.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to configure the client with.</param>
        /// <param name="provider">The <see cref="IServiceProvider"/> to use.</param>
        /// <returns>
        /// The <see cref="ITflClient"/> to use.
        /// </returns>
        private static ITflClient AddTfl(HttpClient client, IServiceProvider provider)
        {
            var settings = provider.GetRequiredService<RefitSettings>();

            client.BaseAddress = new Uri("https://api.tfl.gov.uk/", UriKind.Absolute);

            return RestService.For<ITflClient>(client, settings);
        }

        /// <summary>
        /// Creates an instance of <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <param name="provider">The <see cref="IServiceProvider"/> to use.</param>
        /// <returns>
        /// The created instance of <see cref="JsonSerializerOptions"/>.
        /// </returns>
        private static JsonSerializerOptions CreateJsonSerializerOptions(IServiceProvider provider)
        {
            return new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
        }

        /// <summary>
        /// Creates an instance of <see cref="RefitSettings"/>.
        /// </summary>
        /// <param name="provider">The <see cref="IServiceProvider"/> to use.</param>
        /// <returns>
        /// The created instance of <see cref="RefitSettings"/>.
        /// </returns>
        private static RefitSettings CreateRefitSettings(IServiceProvider provider)
        {
            var contentSerializer = provider.GetRequiredService<IContentSerializer>();
            var messageHandlerFactory = provider.GetRequiredService<IHttpMessageHandlerFactory>();

            return new RefitSettings()
            {
                ContentSerializer = provider.GetRequiredService<IContentSerializer>(),
                HttpMessageHandlerFactory = messageHandlerFactory.CreateHandler,
            };
        }
    }
}
