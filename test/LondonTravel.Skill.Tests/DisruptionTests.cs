// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using JustEat.HttpClientInterception;
using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill;

public class DisruptionTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    [Fact]
    public async Task Can_Invoke_Function_When_There_Are_No_Disruptions()
    {
        // Arrange
        await Interceptor.RegisterBundleAsync(Path.Combine("Bundles", "tfl-no-disruptions.json"));

        AlexaFunction function = await CreateFunctionAsync();
        SkillRequest request = CreateIntentRequest();

        // Act
        SkillResponse actual = await function.HandlerAsync(request);

        // Assert
        AssertResponse(
            actual,
            "<speak>There is currently no disruption on the tube, London Overground, the D.L.R. or the Elizabeth line.</speak>",
            "There is currently no disruption on the tube, London Overground, the DLR or the Elizabeth line.");
    }

    [Fact]
    public async Task Can_Invoke_Function_When_There_Is_One_Disruption()
    {
        // Arrange
        await Interceptor.RegisterBundleAsync(Path.Combine("Bundles", "tfl-one-disruption.json"));

        AlexaFunction function = await CreateFunctionAsync();
        SkillRequest request = CreateIntentRequest();

        // Act
        SkillResponse actual = await function.HandlerAsync(request);

        // Assert
        AssertResponse(
            actual,
            "<speak><p>There are severe delays on the District Line.</p><p>There is a good service on all other lines.</p></speak>",
            "There are severe delays on the District Line.");
    }

    [Fact]
    public async Task Can_Invoke_Function_When_There_Are_Multiple_Disruptions()
    {
        // Arrange
        await Interceptor.RegisterBundleAsync(Path.Combine("Bundles", "tfl-multiple-disruptions.json"));

        AlexaFunction function = await CreateFunctionAsync();
        SkillRequest request = CreateIntentRequest();

        // Act
        SkillResponse actual = await function.HandlerAsync(request);

        // Assert
        AssertResponse(
            actual,
            "<speak><p>Circle Line: There are minor delays on the Circle Line.</p><p>District Line: There are severe delays on the District Line.</p><p>Elizabeth line: There are minor delays on the Elizabeth line.</p><p>Hammersmith and City Line: There are minor delays on the Hammersmith and City Line.</p><p>There is a good service on all other lines.</p></speak>",
            "Circle Line: There are minor delays on the Circle Line.\nDistrict Line: There are severe delays on the District Line.\nElizabeth line: There are minor delays on the Elizabeth line.\nHammersmith & City Line: There are minor delays on the Hammersmith & City Line.");
    }

    [Fact]
    public async Task Can_Invoke_Function_When_The_Api_Fails()
    {
        // Arrange
        AlexaFunction function = await CreateFunctionAsync();
        SkillRequest request = CreateIntentRequest();

        // Act
        SkillResponse actual = await function.HandlerAsync(request);

        // Assert
        ResponseBody response = AssertResponse(actual);

        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");

        var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
        ssml.Ssml.ShouldBe("<speak>Sorry, something went wrong.</speak>");
    }

    private void AssertResponse(SkillResponse actual, string expectedSsml, string expectedCardContent)
    {
        ResponseBody response = AssertResponse(actual);

        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");

        var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
        ssml.Ssml.ShouldBe(expectedSsml);

        response.Card.ShouldNotBeNull();
        var card = response.Card.ShouldBeOfType<StandardCard>();

        card.Type.ShouldBe("Standard");
        card.Title.ShouldBe("Disruption Summary");
        card.Content.ShouldBe(expectedCardContent);
    }

    private SkillRequest CreateIntentRequest()
    {
        return CreateIntentRequest("DisruptionIntent");
    }
}
