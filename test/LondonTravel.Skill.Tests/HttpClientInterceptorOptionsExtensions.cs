// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using JustEat.HttpClientInterception;

namespace MartinCostello.LondonTravel.Skill;

internal static class HttpClientInterceptorOptionsExtensions
{
    public static HttpClientInterceptorOptions RegisterBundleFromResourceStream<T>(
        this HttpClientInterceptorOptions options,
        string name,
        IEnumerable<KeyValuePair<string, string>>? templateValues = default)
    {
        using var stream = GetStream<T>(name);
        return options.RegisterBundleFromStream(stream, templateValues);
    }

    public static async Task<HttpClientInterceptorOptions> RegisterBundleFromResourceStreamAsync<T>(
        this HttpClientInterceptorOptions options,
        string name,
        IEnumerable<KeyValuePair<string, string>>? templateValues = default,
        CancellationToken cancellationToken = default)
    {
        using var stream = GetStream<T>(name);
        return await options.RegisterBundleFromStreamAsync(stream, templateValues, cancellationToken);
    }

    private static Stream GetStream<T>(string name)
    {
        var type = typeof(T);
        var assembly = type.Assembly;
        name = type.Namespace + ".Bundles." + name;

        var stream = assembly.GetManifestResourceStream(name);

        return stream ?? throw new ArgumentException($"The resource '{name}' was not found.", nameof(name));
    }
}
