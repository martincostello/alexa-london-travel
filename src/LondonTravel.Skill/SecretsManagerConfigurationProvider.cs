// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.Lambda.Core;
using Amazon.SecretsManager.Extensions.Caching;
using Microsoft.Extensions.Configuration;

namespace MartinCostello.LondonTravel.Skill;

internal sealed class SecretsManagerConfigurationProvider(SecretsManagerCache cache) : ConfigurationProvider
{
    private static readonly IReadOnlyList<string> SecretNames =
    [
        "OTEL_EXPORTER_OTLP_HEADERS",
        "Skill__SkillId",
        "Skill__TflApplicationId",
        "Skill__TflApplicationKey",
    ];

    public override void Load()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        LoadAsync(cts.Token).GetAwaiter().GetResult();
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        const string Prefix = "alexa-london-travel/";

        var secrets = new Dictionary<string, string?>(SecretNames.Count, StringComparer.OrdinalIgnoreCase);

        foreach (string name in SecretNames)
        {
            string key = name.Replace("__", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);
            string value = await GetSecretAsync($"{Prefix}{name}", cancellationToken);

            if (value is { Length: > 0 })
            {
                secrets[key] = value;
            }
        }

        Data = secrets;
    }

    private async Task<string> GetSecretAsync(string secretId, CancellationToken cancellationToken)
    {
        try
        {
            return await cache.GetSecretString(secretId, cancellationToken);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            // Failed to get the secret
            LambdaLogger.Log($"Failed to retrieve secret ID {secretId} from AWS Secrets Manager: {ex}");
            return string.Empty;
        }
    }
}
