// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using Alexa.NET.Response;

namespace MartinCostello.LondonTravel.Skill;

public class AlexaFunctionHandlerTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    [Fact]
    public async Task Can_Invoke_Static_Function()
    {
        // Arrange
        SkillRequest request = CreateIntentRequest("AMAZON.HelpIntent");

        // Act
        SkillResponse actual = await AlexaFunctionHandler.HandleAsync(request);

        // Assert
        ResponseBody response = AssertResponse(actual, shouldEndSession: false);

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");

        // Act
        actual = await AlexaFunctionHandler.HandleAsync(request);

        // Assert
        response = AssertResponse(actual, shouldEndSession: false);

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
    }
}
