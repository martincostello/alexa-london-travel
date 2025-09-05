// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.Lambda.TestUtilities;

namespace MartinCostello.LondonTravel.Skill;

public class StopTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    [Fact]
    public async Task Can_Invoke_Function()
    {
        // Arrange
        var function = await CreateFunctionAsync();
        var context = new TestLambdaContext();

        var request = CreateIntentRequest("AMAZON.StopIntent");

        // Act
        var actual = await function.HandlerAsync(request, context);

        // Assert
        var response = AssertResponse(actual);

        response.Card.ShouldBeNull();
        response.OutputSpeech.ShouldBeNull();
        response.Reprompt.ShouldBeNull();
    }
}
