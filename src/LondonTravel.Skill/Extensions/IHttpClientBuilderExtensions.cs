// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.LondonTravel.Skill.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="IHttpClientBuilder"/> interface. This class cannot be inherited.
/// </summary>
internal static class IHttpClientBuilderExtensions
{
    /// <summary>
    /// The User Agent to use for all requests. This field is read-only.
    /// </summary>
    private static readonly ProductInfoHeaderValue _userAgent = new("alexa-london-travel", SkillTelemetry.ServiceVersion);

    /// <summary>
    /// Applies the default configuration to the <see cref="IHttpClientBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to apply the default configuration to.</param>
    /// <returns>
    /// The <see cref="IHttpClientBuilder"/> passed as the value of <paramref name="builder"/>.
    /// </returns>
    public static IHttpClientBuilder ApplyDefaultConfiguration(this IHttpClientBuilder builder)
    {
        builder
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                };
            })
            .ConfigureHttpClient((client) =>
            {
                client.DefaultRequestHeaders.UserAgent.Add(_userAgent);
                client.Timeout = TimeSpan.FromSeconds(7.5);
            });

        builder.AddStandardResilienceHandler();

        return builder;
    }
}
