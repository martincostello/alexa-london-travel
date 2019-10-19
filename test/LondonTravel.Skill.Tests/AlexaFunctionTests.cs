// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace MartinCostello.LondonTravel.Skill
{
    public class AlexaFunctionTests
    {
        public AlexaFunctionTests(ITestOutputHelper outputHelper)
        {
            Logger = new XunitLambdaLogger(outputHelper);
        }

        private ILambdaLogger Logger { get; }

        [Fact]
        public async Task Can_Invoke_Function()
        {
            // Arrange
            var config = new SkillConfiguration();
            var function = new AlexaFunction(config);

            var input = new SkillRequest()
            {
                Session = new Session(),
            };

            var context = new TestLambdaContext()
            {
                Logger = Logger,
            };

            // Act
            SkillResponse actual = await function.HandlerAsync(input, context);

            // Assert
            actual.ShouldNotBeNull();
            actual.Version.ShouldBe("1.0");
            actual.Response.ShouldNotBeNull();
            actual.Response.ShouldEndSession.ShouldBe(true);
        }
    }
}
