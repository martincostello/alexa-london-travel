// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Xunit.Abstractions;

namespace MartinCostello.LondonTravel.Skill
{
    public class LaunchTests : FunctionTests
    {
        public LaunchTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async Task Can_Invoke_Function()
        {
            // Arrange
            AlexaFunction function = await CreateFunctionAsync();

            SkillRequest request = CreateRequest<LaunchRequest>();
            ILambdaContext context = CreateContext();

            // Act
            SkillResponse actual = await function.HandlerAsync(request, context);

            // Assert
            ResponseBody response = AssertResponse(actual, shouldEndSession: false);

            response.Card.ShouldBeNull();
            response.Reprompt.ShouldBeNull();

            response.OutputSpeech.ShouldNotBeNull();
            response.OutputSpeech.Type.ShouldBe("SSML");

            var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
            ssml.Ssml.ShouldBe("<speak>Welcome to London Travel. You can ask me about disruption or for the status of any tube line, London Overground, the D.L.R. or T.F.L. Rail.</speak>");
        }
    }
}
