// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.Lambda.TestUtilities;
using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill;

public class UnknownIntentTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    [Fact]
    public async Task Can_Invoke_Function()
    {
        // Arrange
        var function = await CreateFunctionAsync();
        var context = new TestLambdaContext();

        var request = CreateIntentRequest("FooIntent");

        // Act
        var actual = await function.HandlerAsync(request, context);

        // Assert
        var response = AssertResponse(actual);

        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
        response.OutputSpeech.Ssml.ShouldBe("<speak>Sorry, I don't understand how to do that.</speak>");
    }

    private sealed class UnknownRequest : Request
    {
        public override string Type => "Unknown";
    }
}
