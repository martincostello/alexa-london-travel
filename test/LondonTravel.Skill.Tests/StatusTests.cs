// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using Alexa.NET.Response;
using JustEat.HttpClientInterception;

namespace MartinCostello.LondonTravel.Skill;

[UsesVerify]
public class StatusTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    [Fact]
    public async Task Can_Invoke_Function_For_Valid_Line()
    {
        // Arrange
        await Interceptor.RegisterBundleAsync(Path.Combine("Bundles", "tfl-line-statuses.json"));

        AlexaFunction function = await CreateFunctionAsync();
        SkillRequest request = CreateIntentForLine("northern");

        // Act
        SkillResponse actual = await function.HandlerAsync(request);

        // Assert
        await Verify(actual);
    }

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
    [InlineData("London Overground")]
    [InlineData("Overground")]
    [InlineData("Met")]
    [InlineData("Metropolitan")]
    [InlineData("Northern")]
    [InlineData("Piccadilly")]
    [InlineData("TfL Rail")]
    [InlineData("Victoria")]
    [InlineData("City")]
    [InlineData("Waterloo")]
    [InlineData("Waterloo & City")]
    [InlineData("Waterloo and City")]
    public async Task Can_Invoke_Function_For_Valid_Lines(string id)
    {
        // Arrange
        await Interceptor.RegisterBundleAsync(Path.Combine("Bundles", "tfl-line-statuses.json"));

        AlexaFunction function = await CreateFunctionAsync();
        SkillRequest request = CreateIntentForLine(id);

        // Act
        SkillResponse actual = await function.HandlerAsync(request);

        // Assert
        AssertLineResponse(actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("not a tube line")]
    public async Task Can_Invoke_Function_For_Invalid_Line(string id)
    {
        // Arrange
        AlexaFunction function = await CreateFunctionAsync();
        SkillRequest request = CreateIntentForLine(id);

        // Act
        SkillResponse actual = await function.HandlerAsync(request);

        // Assert
        ResponseBody response = AssertResponse(actual);

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

            var ssml = speech.ShouldBeOfType<SsmlOutputSpeech>();
            ssml.Ssml.ShouldBe("<speak>Sorry, I am not sure what line you said. You can ask about the status of any tube line, London Overground, the D.L.R. or the Elizabeth line.</speak>");
        }
    }

    [Fact]
    public async Task Can_Invoke_Function_When_The_Api_Fails()
    {
        // Arrange
        AlexaFunction function = await CreateFunctionAsync();
        SkillRequest request = CreateIntentForLine("district");

        // Act
        SkillResponse actual = await function.HandlerAsync(request);

        // Assert
        await Verify(actual);

        ResponseBody response = AssertResponse(actual);

        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");

        var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
        ssml.Ssml.ShouldBe("<speak>Sorry, something went wrong.</speak>");
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
    [InlineData("London Overground", "There is a good service on London Overground.")]
    [InlineData("Metropolitan", "There is a good service on the Metropolitan line.")]
    [InlineData("Northern", "There is a good service on the Northern line.")]
    [InlineData("Piccadilly", "There is a good service on the Piccadilly line.")]
    [InlineData("TfL Rail", "There is a good service on T.F.L. Rail.")]
    [InlineData("Victoria", "There is a good service on the Victoria line.")]
    [InlineData("Waterloo & City", "There is a good service on the Waterloo and City line.")]
    public async Task Can_Invoke_Function_For_Different_Severities(
        string id,
        string expected)
    {
        // Arrange
        await Interceptor.RegisterBundleAsync(Path.Combine("Bundles", "tfl-line-severities.json"));

        AlexaFunction function = await CreateFunctionAsync();
        SkillRequest request = CreateIntentForLine(id);

        // Act
        SkillResponse actual = await function.HandlerAsync(request);

        // Assert
        AssertLineResponse(actual, expectedSsml: "<speak>" + expected + "</speak>");
    }

    private void AssertLineResponse(
        SkillResponse actual,
        string expectedSsml = null,
        string expectedCardTitle = null,
        string expectedCardContent = null)
    {
        ResponseBody response = AssertResponse(actual);

        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");

        if (expectedSsml != null)
        {
            var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
            ssml.Ssml.ShouldBe(expectedSsml);
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

    private SkillRequest CreateIntentForLine(string id)
    {
        return CreateIntentRequest(
            "StatusIntent",
            new Slot() { Name = "LINE", Value = id });
    }
}
