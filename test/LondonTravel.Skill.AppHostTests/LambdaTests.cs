// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Text.Json;
using System.Xml.Linq;
using Amazon.Lambda.Model;
using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill.AppHostTests;

[Collection<LambdaFunctionCollection>]
public sealed class LambdaTests
{
    private const string PayloadPrefix = "Payload-";
    private const int TimeoutMilliseconds = 45_000;

    public LambdaTests(LambdaFunctionFixture fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;
        Fixture.OutputHelper = outputHelper;
    }

    private LambdaFunctionFixture Fixture { get; }

    public static TheoryData<string> Payloads()
    {
        var payloads = new TheoryData<string>();

        foreach (string path in typeof(LambdaTests).Assembly.GetManifestResourceNames().Order())
        {
            payloads.Add(path[PayloadPrefix.Length..]);
        }

        return payloads;
    }

    [Theory(Timeout = TimeoutMilliseconds)]
    [MemberData(nameof(Payloads))]
    public async Task Can_Invoke_Intent_Can_Get_Json_Response(string payloadName)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        var request = new InvokeRequest()
        {
            FunctionName = "LondonTravelSkill",
            Payload = await GetPayloadAsync(payloadName, cancellationToken),
        };

        using var client = Fixture.CreateClient();

        // Act
        var invocation = await client.InvokeAsync(request, cancellationToken);

        // Assert
        invocation.ShouldNotBeNull();
        invocation.ResponseMetadata.ShouldNotBeNull();
        invocation.HttpStatusCode.ShouldBe(HttpStatusCode.OK);
        invocation.StatusCode.ShouldBe(200);
        invocation.FunctionError.ShouldBeNull();

        var actual = await JsonSerializer.DeserializeAsync(
            invocation.Payload,
            AppJsonSerializerContext.Default.SkillResponse,
            cancellationToken: cancellationToken);

        actual.ShouldNotBeNull();
        actual.Version.ShouldBe("1.0");
        actual.Response.ShouldNotBeNull();

        if (actual.Response.Card is { } card)
        {
            card.ShouldBeOfType<StandardCard>();
        }

        if (actual.Response.OutputSpeech is { } speech)
        {
            speech.Type.ShouldBe("SSML");
            speech.Ssml.ShouldNotContain("Sorry, something went wrong.");
            speech.Ssml.ShouldStartWith("<speak>");
            speech.Ssml.ShouldEndWith("</speak>");

            XDocument.Parse(speech.Ssml).ShouldNotBeNull();
        }
    }

    private static async Task<string> GetPayloadAsync(string name, CancellationToken cancellationToken)
    {
        var assembly = typeof(LambdaTests).Assembly;

        using var stream = assembly.GetManifestResourceStream(PayloadPrefix + name);
        stream.ShouldNotBeNull();

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
