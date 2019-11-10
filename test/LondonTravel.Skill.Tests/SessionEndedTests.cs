// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace MartinCostello.LondonTravel.Skill
{
    public class SessionEndedTests : FunctionTests
    {
        public SessionEndedTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async Task Can_Invoke_Function()
        {
            // Arrange
            AlexaFunction function = await CreateFunctionAsync();

            SkillRequest request = CreateRequest<SessionEndedRequest>();
            ILambdaContext context = CreateContext();

            // Act
            SkillResponse actual = await function.HandlerAsync(request, context);

            // Assert
            ResponseBody response = AssertResponse(actual);

            response.Card.ShouldBeNull();
            response.Reprompt.ShouldBeNull();

            response.OutputSpeech.ShouldNotBeNull();
            response.OutputSpeech.Type.ShouldBe("SSML");

            var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
            ssml.Ssml.ShouldBe("<speak>Goodbye.</speak>");
        }
    }
}
