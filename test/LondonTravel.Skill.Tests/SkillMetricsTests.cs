// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.LondonTravel.Skill;

public static class SkillMetricsTests
{
    [Fact]
    public static void SkillMetrics_Initializes_With_Default_Values()
    {
        // Arrange
        using var serviceProvider = new ServiceCollection()
            .AddMetrics()
            .BuildServiceProvider();

        var meterFactory = serviceProvider.GetRequiredService<IMeterFactory>();
        using var target = new SkillMetrics(meterFactory);

        // Act and Assert
        Should.NotThrow(() => target.SkillInvoked("Launch"));
    }
}
