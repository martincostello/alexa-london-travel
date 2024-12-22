// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.Lambda.TestUtilities;
using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill;

public class StatusTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    [Theory]
    [InlineData("Bakerloo")]
    [InlineData("bakerloo")]
    [InlineData("BAKERLOO")]
    [InlineData("Central")]
    [InlineData("Circle")]
    [InlineData("crossrail")]
    [InlineData("District")]
    [InlineData("DLR")]
    [InlineData("Docklands")]
    [InlineData("Docklands Light Rail")]
    [InlineData("Docklands Light Railway")]
    [InlineData("Docklands Rail")]
    [InlineData("Docklands Railway")]
    [InlineData("Elizabeth")]
    [InlineData("Elizabeth Line")]
    [InlineData("Elizabeth line")]
    [InlineData("Hammersmith")]
    [InlineData("Hammersmith & City")]
    [InlineData("Hammersmith and City")]
    [InlineData("Jubilee")]
    [InlineData("Liberty")]
    [InlineData("Lioness")]
    [InlineData("Met")]
    [InlineData("Metropolitan")]
    [InlineData("Mildmay")]
    [InlineData("Northern")]
    [InlineData("Piccadilly")]
    [InlineData("Suffragette")]
    [InlineData("Victoria")]
    [InlineData("City")]
    [InlineData("Waterloo")]
    [InlineData("Waterloo & City")]
    [InlineData("Waterloo and City")]
    [InlineData("Weaver")]
    [InlineData("Windrush")]
    public async Task Can_Invoke_Function_For_Valid_Lines(string id)
    {
        // Arrange
        await Interceptor.RegisterBundleFromResourceStreamAsync<StatusTests>(
            "tfl-line-statuses.json",
            cancellationToken: TestContext.Current.CancellationToken);

        var function = await CreateFunctionAsync();
        var request = CreateIntentForLine(id);
        var context = new TestLambdaContext();

        // Act
        var actual = await function.HandlerAsync(request, context);

        // Assert
        AssertLineResponse(actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("not a tube line")]
    [InlineData("London Overground")]
    [InlineData("Overground")]
    public async Task Can_Invoke_Function_For_Invalid_Line(string? id)
    {
        // Arrange
        var function = await CreateFunctionAsync();
        var request = CreateIntentForLine(id);
        var context = new TestLambdaContext();

        // Act
        var actual = await function.HandlerAsync(request, context);

        // Assert
        var response = AssertResponse(actual);

        response.Card.ShouldBeNull();
        response.OutputSpeech.ShouldNotBeNull();
        response.Reprompt.ShouldNotBeNull();

        var speeches = new[]
        {
            response.OutputSpeech,
            response.Reprompt.OutputSpeech,
        };

        foreach (var speech in speeches)
        {
            speech.Type.ShouldBe("SSML");
            speech.Ssml.ShouldBe("<speak>Sorry, I am not sure what line you said. You can ask about the status of any tube line, London Overground, the D.L.R. or the Elizabeth line.</speak>");
        }
    }

    [Fact]
    public async Task Can_Invoke_Function_When_The_Api_Fails()
    {
        // Arrange
        var function = await CreateFunctionAsync();
        var request = CreateIntentForLine("district");
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

    [Theory]
    [InlineData("Bakerloo", "The Bakerloo line is closed.")]
    [InlineData("Central", "There is a good service on the Central line.")]
    [InlineData("Circle", "Saturday 19 and Sunday 20 October, no service between Edgware Road and Aldgate (via Victoria).")]
    [InlineData("District", "There is a good service on the District line.")]
    [InlineData("DLR", "There is a good service on the D.L.R..")]
    [InlineData("Elizabeth line", "There is a good service on the Elizabeth line.")]
    [InlineData("Hammersmith & City", "There is a good service on the Hammersmith and City line.")]
    [InlineData("Jubilee", "There is a good service on the Jubilee line.")]
    [InlineData("Liberty", "There is a good service on the Liberty line.")]
    [InlineData("Lioness", "There is a good service on the Lioness line.")]
    [InlineData("Metropolitan", "There is a good service on the Metropolitan line.")]
    [InlineData("Mildmay", "There is a good service on the Mildmay line.")]
    [InlineData("Northern", "There is a good service on the Northern line.")]
    [InlineData("Piccadilly", "There is a good service on the Piccadilly line.")]
    [InlineData("Suffragette", "There is a good service on the Suffragette line.")]
    [InlineData("Victoria", "There is a good service on the Victoria line.")]
    [InlineData("Waterloo & City", "There is a good service on the Waterloo and City line.")]
    [InlineData("Weaver", "There is a good service on the Weaver line.")]
    [InlineData("Windrush", "There is a good service on the Windrush line.")]
    public async Task Can_Invoke_Function_For_Different_Severities(
        string id,
        string expected)
    {
        // Arrange
        await Interceptor.RegisterBundleFromResourceStreamAsync<StatusTests>(
            "tfl-line-severities.json",
            cancellationToken: TestContext.Current.CancellationToken);

        var function = await CreateFunctionAsync();
        var request = CreateIntentForLine(id);
        var context = new TestLambdaContext();

        // Act
        var actual = await function.HandlerAsync(request, context);

        // Assert
        AssertLineResponse(actual, expectedSsml: "<speak>" + expected + "</speak>");
    }

    private void AssertLineResponse(
        SkillResponse actual,
        string? expectedSsml = null,
        string? expectedCardTitle = null,
        string? expectedCardContent = null)
    {
        var response = AssertResponse(actual);

        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");

        if (expectedSsml != null)
        {
            response.OutputSpeech.Ssml.ShouldBe(expectedSsml);
        }

        response.Card.ShouldNotBeNull();
        var card = response.Card.ShouldBeOfType<StandardCard>();

        card.Type.ShouldBe("Standard");

        if (expectedCardTitle != null)
        {
            card.Title.ShouldBe(expectedCardTitle);
        }

        if (expectedCardContent != null)
        {
            card.Content.ShouldBe(expectedCardContent);
        }
    }

    private SkillRequest CreateIntentForLine(string? id)
    {
        return CreateIntentRequest(
            "StatusIntent",
            new Slot() { Name = "LINE", Value = id! });
    }
}
