// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.SecretsManager.Extensions.Caching;
using Microsoft.Extensions.Configuration;

#pragma warning disable CA2000

namespace MartinCostello.LondonTravel.Skill.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="IConfigurationBuilder"/> interface. This class cannot be inherited.
/// </summary>
public static class IConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds AWS Secrets Manager to the specified <see cref="IConfigurationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>
    /// The value specified by <paramref name="builder"/>.
    /// </returns>
    public static IConfigurationBuilder AddSecretsManager(this IConfigurationBuilder builder)
        => builder.AddSecretsManager(new());

    /// <summary>
    /// Adds AWS Secrets Manager to the specified <see cref="IConfigurationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <param name="cache">The <see cref="SecretsManagerCache"/> to use.</param>
    /// <returns>
    /// The value specified by <paramref name="builder"/>.
    /// </returns>
    public static IConfigurationBuilder AddSecretsManager(this IConfigurationBuilder builder, SecretsManagerCache cache)
        => builder.Add(new SecretsManagerConfigurationSource(cache));
}
