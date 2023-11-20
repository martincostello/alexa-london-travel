// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Skill.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Skill.Extensions;

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
            .ApplyDefaultConfiguration()
            .AddStandardResilienceHandler();

        services
            .AddHttpClient<SkillClient>((provider, client) =>
            {
                var config = provider.GetRequiredService<SkillConfiguration>();
                client.BaseAddress = new Uri(config.SkillApiUrl, UriKind.Absolute);
            })
            .ApplyDefaultConfiguration()
            .AddStandardResilienceHandler();

        services
            .AddHttpClient<TflClient>((p) => p.BaseAddress = new Uri("https://api.tfl.gov.uk/", UriKind.Absolute))
            .ApplyDefaultConfiguration()
            .AddStandardResilienceHandler();

        return services;
    }
}
