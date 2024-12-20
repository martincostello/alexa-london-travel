// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.Lambda.TestUtilities;
using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill;

public class UnknownRequestTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    [Fact]
    public async Task Can_Invoke_Function()
    {
        // Arrange
        var function = await CreateFunctionAsync();
        var context = new TestLambdaContext();

        var request = CreateRequest<UnknownRequest>();

        // Act
        var actual = await function.HandlerAsync(request, context);

        // Assert
        AssertResponse(actual);
    }

    private sealed class UnknownRequest : Request
    {
        public override string Type => "Unknown";
    }
}
