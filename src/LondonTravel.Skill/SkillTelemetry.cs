// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection;
using OpenTelemetry.Resources;

namespace MartinCostello.LondonTravel.Skill;

internal static class SkillTelemetry
{
    public const string ServiceName = "LondonTravel.Skill";
    public const string ServiceNamespace = "LondonTravel";

    public static readonly string ServiceVersion = GetVersion<AlexaFunction>();
    public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

    public static ResourceBuilder ResourceBuilder { get; } = ResourceBuilder.CreateDefault()
        .AddService(ServiceName, ServiceNamespace, ServiceVersion)
        .AddHostDetector()
        .AddOperatingSystemDetector()
        .AddProcessRuntimeDetector();

    private static string GetVersion<T>()
        => typeof(T).Assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
                    .InformationalVersion;
}
