// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace MartinCostello.LondonTravel.Skill
{
    public class CancelTests : FunctionTests
    {
        public CancelTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async Task Can_Invoke_Function()
        {
            // Arrange
            AlexaFunction function = await CreateFunctionAsync();

            SkillRequest request = CreateIntentRequest("AMAZON.CancelIntent");
            ILambdaContext context = CreateContext();

            // Act
            SkillResponse actual = await function.HandlerAsync(request, context);

            // Assert
            ResponseBody response = AssertResponse(actual);

            response.Card.ShouldBeNull();
            response.OutputSpeech.ShouldBeNull();
            response.Reprompt.ShouldBeNull();
        }
    }
}
