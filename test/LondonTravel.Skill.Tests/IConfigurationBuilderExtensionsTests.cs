// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Skill.Extensions;
using Microsoft.Extensions.Configuration;

namespace MartinCostello.LondonTravel.Skill;

public static class IConfigurationBuilderExtensionsTests
{
    [Fact]
    public static void AddSecretsManager_Adds_Configuration_Source()
    {
        // Arrange
        var builder = new ConfigurationBuilder();

        try
        {
            Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_SECRETS_MANAGER", "http://aws.local");

            // Act
            builder.AddSecretsManager();
        }
        finally
        {
            Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_SECRETS_MANAGER", null);
        }

        // Assert
        builder.Sources.Count.ShouldBe(1);
        builder.Sources[0].GetType().Name.ShouldBe("SecretsManagerConfigurationSource");

        // Act and Assert
        Should.NotThrow(builder.Build);

        // Act and Assert
        Should.NotThrow(() =>
        {
            foreach (var source in builder.Sources)
            {
                if (source is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        });
    }
}
