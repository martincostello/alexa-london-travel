// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Http.Headers;
using System.Reflection;
using Amazon.Runtime;

namespace MartinCostello.LondonTravel.Skill.EndToEndTests;

internal static class TestConfiguration
{
    public static string? AlexaClientId => Environment.GetEnvironmentVariable("LWA_CLIENT_ID");

    public static string? AlexaClientSecret => Environment.GetEnvironmentVariable("LWA_CLIENT_SECRET");

    public static string? AlexaRefreshToken => Environment.GetEnvironmentVariable("LWA_REFRESH_TOKEN");

    public static string? FunctionName => Environment.GetEnvironmentVariable("LAMBDA_FUNCTION_NAME");

    public static string? RegionName => Environment.GetEnvironmentVariable("AWS_REGION");

    public static string? SkillId => Environment.GetEnvironmentVariable("SKILL_ID");

    public static string? SkillStage => Environment.GetEnvironmentVariable("SKILL_STAGE");

    public static ProductInfoHeaderValue UserAgent { get; } = new("LondonTravel.Skill.EndToEndTests", typeof(TestConfiguration).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion);

    public static AWSCredentials? GetCredentials()
    {
        try
        {
            return new EnvironmentVariablesAWSCredentials();
        }
        catch (InvalidOperationException)
        {
            // Not configured
        }

        try
        {
            return AssumeRoleWithWebIdentityCredentials.FromEnvironmentVariables();
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
