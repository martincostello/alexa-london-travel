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

        using var lambdaClient = new AmazonLambdaClient(credentials, region);

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
        InvokeResponse invocation = await lambdaClient.InvokeAsync(request);

        using var reader = new StreamReader(invocation.Payload);
        string responsePayload = await reader.ReadToEndAsync();

        outputHelper.WriteLine($"ExecutedVersion: {invocation.ExecutedVersion}");
        outputHelper.WriteLine($"FunctionError: {invocation.FunctionError}");
        outputHelper.WriteLine($"HttpStatusCode: {invocation.HttpStatusCode}");
        outputHelper.WriteLine($"RequestId: {invocation.ResponseMetadata.RequestId}");
        outputHelper.WriteLine($"StatusCode: {invocation.StatusCode}");
        outputHelper.WriteLine($"Payload: {responsePayload}");

        // Assert
        invocation.HttpStatusCode.ShouldBe(HttpStatusCode.OK);
        invocation.StatusCode.ShouldBe(200);
        invocation.FunctionError.ShouldBeNull();
        invocation.ExecutedVersion.ShouldBe("$LATEST");

        using var document = JsonDocument.Parse(responsePayload);

        document.RootElement.ValueKind.ShouldBe(JsonValueKind.Object);
        document.RootElement.ToString().ShouldNotContain("Sorry, something went wrong.");

        document.RootElement.TryGetProperty("version", out var version).ShouldBeTrue();
        version.GetString().ShouldBe("1.0");

        document.RootElement.TryGetProperty("response", out var response).ShouldBeTrue();
        response.TryGetProperty("shouldEndSession", out _).ShouldBeTrue();

        if (response.TryGetProperty("outputSpeech", out var speech))
        {
            speech.TryGetProperty("type", out var type).ShouldBeTrue();
            type.GetString().ShouldBe("SSML");

            speech.TryGetProperty("ssml", out var ssml).ShouldBeTrue();
            ssml.GetString().ShouldNotBeNullOrWhiteSpace();
        }
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
