// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.TestUtilities;
using Shouldly;
using Xunit;

namespace MartinCostello.LondonTravel.Skill
{
    public static class AlexaSkillTests
    {
        [Fact]
        public static async Task Can_Invoke_Function()
        {
            // Arrange
            var function = new AlexaSkill();

            var input = new SkillRequest();
            var context = new TestLambdaContext();

            // Act
            SkillResponse actual = await function.HandlerAsync(input, context);

            // Assert
            actual.ShouldNotBeNull();
        }
    }
}
