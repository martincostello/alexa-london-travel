// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Text.Json;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using LondonTravel.Skill.EndToEndTests;

namespace MartinCostello.LondonTravel.Skill;

[Collection(CloudWatchLogsFixtureCollection.Name)]
public class LambdaTests(CloudWatchLogsFixture fixture, ITestOutputHelper outputHelper)
{
    public static TheoryData<string> Payloads()
    {
        var payloads = new List<string>()
        {
            "Disruption",
            "Status",
        };

        return new(payloads);
    }

    [SkippableTheory]
    [MemberData(nameof(Payloads))]
    public async Task Can_Invoke_Intent_Can_Get_Json_Response(string payloadName)
    {
        var credentials = TestConfiguration.GetCredentials();

        Skip.If(credentials is null, "No AWS credentials are configured.");

        string functionName = TestConfiguration.FunctionName;
        string regionName = TestConfiguration.RegionName;

        Skip.If(string.IsNullOrEmpty(functionName), "No Lambda function name is configured.");
        Skip.If(string.IsNullOrEmpty(regionName), "No AWS region name is configured.");

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
        InvokeResponse invocation = await client.InvokeAsync(request);

        // Assert
        invocation.ShouldNotBeNull();
        invocation.ResponseMetadata.ShouldNotBeNull();

        fixture.Requests[invocation.ResponseMetadata.RequestId] = payloadName;

        using var reader = new StreamReader(invocation.Payload);
        string responsePayload = await reader.ReadToEndAsync();

        outputHelper.WriteLine($"ExecutedVersion: {invocation.ExecutedVersion}");
        outputHelper.WriteLine($"FunctionError: {invocation.FunctionError}");
        outputHelper.WriteLine($"HttpStatusCode: {invocation.HttpStatusCode}");
        outputHelper.WriteLine($"RequestId: {invocation.ResponseMetadata.RequestId}");
        outputHelper.WriteLine($"StatusCode: {invocation.StatusCode}");
        outputHelper.WriteLine($"Payload: {responsePayload}");

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
}
