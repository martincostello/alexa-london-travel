// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection;

namespace MartinCostello.LondonTravel.Skill;

internal static class SkillTelemetry
{
    public const string ServiceName = "LondonTravel.Skill";

    public static readonly string ServiceVersion = GetVersion<AlexaFunction>();
    public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

    private static string GetVersion<T>()
    {
        return typeof(T).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;
    }
}
