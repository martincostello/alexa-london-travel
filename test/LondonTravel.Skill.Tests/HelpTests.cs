// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.Lambda.TestUtilities;

namespace MartinCostello.LondonTravel.Skill;

public class HelpTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    [Fact]
    public async Task Can_Invoke_Function()
    {
        // Arrange
        var function = await CreateFunctionAsync();

        var request = CreateIntentRequest("AMAZON.HelpIntent");
        var context = new TestLambdaContext();

        // Act
        var actual = await function.HandlerAsync(request, context);

        // Assert
        var response = AssertResponse(actual, shouldEndSession: false);

        response.Card.ShouldBeNull();
        response.Reprompt.ShouldBeNull();

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
        response.OutputSpeech.Ssml.ShouldBe("<speak><p>This skill allows you to check for the status of a specific line, or for disruption in general. You can ask about any London Underground line, London Overground, the Docklands Light Railway or the Elizabeth line.</p><p>Asking about disruption in general provides information about any lines that are currently experiencing issues, such as any delays or planned closures.</p><p>Asking for the status for a specific line provides a summary of the current service, such as whether there is a good service or if there are any delays.</p><p>If you link your account and setup your preferences in the London Travel website, you can ask about your commute to quickly find out the status of the lines you frequently use.</p><p>What would you like to do?</p></speak>");
    }
}
