// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;

namespace MartinCostello.LondonTravel.Skill;

public class UnknownIntentTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    [Fact]
    public async Task Can_Invoke_Function()
    {
        // Arrange
        await using var function = new AlexaFunction();
        await function.InitializeAsync();

        SkillRequest request = CreateIntentRequest("FooIntent");

        // Act
        SkillResponse actual = await function.HandlerAsync(request);

        // Assert
        ResponseBody response = AssertResponse(actual);

        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");

        var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
        ssml.Ssml.ShouldBe("<speak>Sorry, I don't understand how to do that.</speak>");
    }

    private sealed class UnknownRequest : Request
    {
    }
}
