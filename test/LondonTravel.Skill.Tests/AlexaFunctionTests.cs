// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill;

[UsesVerify]
public class AlexaFunctionTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    [Fact]
    public async Task Cannot_Invoke_Function_If_Application_Id_Incorrect()
    {
        // Arrange
        AlexaFunction function = await CreateFunctionAsync();

        SkillRequest request = CreateIntentRequest("AMAZON.HelpIntent");
        request.Session.Application.ApplicationId = "not-my-skill-id";

        // Act
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => function.HandlerAsync(request));

        // Assert
        exception.Message.ShouldBe("Request application Id 'not-my-skill-id' and configured skill Id 'my-skill-id' mismatch.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("____")]
    [InlineData("qps-Ploc")]
    public async Task Can_Invoke_Function_If_Locale_Is_Invalid(string locale)
    {
        // Arrange
        AlexaFunction function = await CreateFunctionAsync();

        SkillRequest request = CreateIntentRequest("AMAZON.HelpIntent");
        request.Request.Locale = locale;

        // Act
        SkillResponse actual = await function.HandlerAsync(request);

        // Assert
        ResponseBody response = AssertResponse(actual, shouldEndSession: false);

        response.OutputSpeech.ShouldNotBeNull();
        response.OutputSpeech.Type.ShouldBe("SSML");
    }

    [Fact]
    public async Task Cannot_Invoke_Function_With_System_Failure()
    {
        // Arrange
        AlexaFunction function = await CreateFunctionAsync();

        var error = new SystemExceptionRequest()
        {
            Error = new()
            {
                Message = "Internal error.",
                Type = AlexaErrorType.InternalError,
            },
            ErrorCause = new()
            {
                RequestId = "my-request-id",
            },
        };

        var request = CreateRequest(error);

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
}
