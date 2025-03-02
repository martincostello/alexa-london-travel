// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.SecretsManager.Extensions.Caching;
using Microsoft.Extensions.Configuration;

namespace MartinCostello.LondonTravel.Skill;

internal sealed class SecretsManagerConfigurationSource(SecretsManagerCache cache) : IConfigurationSource, IDisposable
{
    private readonly SecretsManagerCache _cache = cache;
    private bool _disposed;

    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new SecretsManagerConfigurationProvider(_cache);

    public void Dispose()
    {
        if (!_disposed)
        {
            _cache.Dispose();
            _disposed = true;
        }
    }
}
