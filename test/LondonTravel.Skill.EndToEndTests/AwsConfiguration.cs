// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.Runtime;

namespace LondonTravel.Skill.EndToEndTests;

internal static class AwsConfiguration
{
    public static string FunctionName => Environment.GetEnvironmentVariable("LAMBDA_FUNCTION_NAME");

    public static string RegionName => Environment.GetEnvironmentVariable("AWS_REGION");

    public static AWSCredentials GetCredentials()
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
