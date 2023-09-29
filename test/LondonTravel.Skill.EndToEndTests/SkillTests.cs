// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Text.Json;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using Environment = System.Environment;

namespace MartinCostello.LondonTravel.Skill;

public class SkillTests(ITestOutputHelper outputHelper)
{
    public static IEnumerable<object[]> Payloads
    {
        get
        {
            return Directory.GetFiles("Payloads")
                .Select(Path.GetFileNameWithoutExtension)
                .Order()
                .Select((p) => new object[] { p })
                .ToArray();
        }
    }

    [SkippableTheory]
    [MemberData(nameof(Payloads))]
    public async Task Can_Invoke_Intent_Can_Get_Json_Response(string payloadName)
    {
        var credentials = GetAwsCredentials();

        Skip.If(credentials is null, "No AWS credentials are configured.");

        string functionName = Environment.GetEnvironmentVariable("LAMBDA_FUNCTION_NAME");

        Skip.If(
            string.IsNullOrEmpty(functionName),
            "No Lambda function name is configured.");

        string regionName = Environment.GetEnvironmentVariable("AWS_REGION");

        Skip.If(
            string.IsNullOrEmpty(regionName),
            "No AWS region name is configured.");

        // Arrange
        string payload = await File.ReadAllTextAsync(Path.Combine("Payloads", $"{payloadName}.json"));

        var region = RegionEndpoint.GetBySystemName(regionName);

        using var client = new AmazonLambdaClient(credentials, region);

        var request = new InvokeRequest()
        {
            FunctionName = functionName,
            InvocationType = InvocationType.RequestResponse,
            LogType = LogType.None,
            Payload = payload,
        };

        outputHelper.WriteLine($"FunctionName: {request.FunctionName}");
        outputHelper.WriteLine($"Payload: {request.Payload}");

        // Act
        InvokeResponse response = await client.InvokeAsync(request);

        using var reader = new StreamReader(response.Payload);
        string responsePayload = await reader.ReadToEndAsync();

        outputHelper.WriteLine($"ExecutedVersion: {response.ExecutedVersion}");
        outputHelper.WriteLine($"FunctionError: {response.FunctionError}");
        outputHelper.WriteLine($"HttpStatusCode: {response.HttpStatusCode}");
        outputHelper.WriteLine($"RequestId: {response.ResponseMetadata.RequestId}");
        outputHelper.WriteLine($"StatusCode: {response.StatusCode}");

        // Assert
        response.HttpStatusCode.ShouldBe(HttpStatusCode.OK);
        response.StatusCode.ShouldBe(200);
        response.FunctionError.ShouldBeNull();
        response.ExecutedVersion.ShouldBe("$LATEST");

        using var document = JsonDocument.Parse(responsePayload);

        document.RootElement.ValueKind.ShouldBe(JsonValueKind.Object);
        document.RootElement.ToString().ShouldNotContain("Sorry, something went wrong.");
    }

    private static AWSCredentials GetAwsCredentials()
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
