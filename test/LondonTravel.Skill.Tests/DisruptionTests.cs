// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.Lambda.TestUtilities;
using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill;

public class DisruptionTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    [Fact]
    public async Task Can_Invoke_Function_When_There_Are_No_Disruptions()
    {
        // Arrange
        await Interceptor.RegisterBundleFromResourceStreamAsync<DisruptionTests>(
            "tfl-no-disruptions.json",
            cancellationToken: TestContext.Current.CancellationToken);

        var function = await CreateFunctionAsync();
        var request = CreateIntentRequest();
        var context = new TestLambdaContext();

        // Act
        var actual = await function.HandlerAsync(request, context);

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
        await Interceptor.RegisterBundleFromResourceStreamAsync<DisruptionTests>(
            "tfl-one-disruption.json",
            cancellationToken: TestContext.Current.CancellationToken);

        var function = await CreateFunctionAsync();
        var request = CreateIntentRequest();
        var context = new TestLambdaContext();

        // Act
        var actual = await function.HandlerAsync(request, context);

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
        await Interceptor.RegisterBundleFromResourceStreamAsync<DisruptionTests>(
            "tfl-multiple-disruptions.json",
            cancellationToken: TestContext.Current.CancellationToken);

        var function = await CreateFunctionAsync();
        var request = CreateIntentRequest();
        var context = new TestLambdaContext();

        // Act
        var actual = await function.HandlerAsync(request, context);

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
        var function = await CreateFunctionAsync();
        var request = CreateIntentRequest();
        var context = new TestLambdaContext();

        // Act
        var actual = await function.HandlerAsync(request, context);

        // Assert
        var response = AssertResponse(actual);

        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
        response.OutputSpeech.Ssml.ShouldBe("<speak>Sorry, something went wrong.</speak>");
    }

    private void AssertResponse(SkillResponse actual, string expectedSsml, string expectedCardContent)
    {
        var response = AssertResponse(actual);

        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
        response.OutputSpeech.Ssml.ShouldBe(expectedSsml);

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
