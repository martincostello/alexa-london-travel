// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;

namespace MartinCostello.LondonTravel.Skill;

public class UnknownRequestTests(ITestOutputHelper outputHelper) : FunctionTests(outputHelper)
{
    [Fact]
    public async Task Can_Invoke_Function()
    {
        // Arrange
        AlexaFunction function = await CreateFunctionAsync();

        SkillRequest request = CreateRequest<UnknownRequest>();
        ILambdaContext context = CreateContext();

        // Act
        SkillResponse actual = await function.HandlerAsync(request, context);

        // Assert
        AssertResponse(actual);
    }

    private sealed class UnknownRequest : Request
    {
    }
}
