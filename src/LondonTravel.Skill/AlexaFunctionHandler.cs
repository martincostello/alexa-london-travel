// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using Alexa.NET.Response;

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class containing the static handler function for the Lambda. This class cannot be inherited.
/// </summary>
public static class AlexaFunctionHandler
{
    private static readonly AlexaFunction _function = new();
    private static bool _initialized;

    /// <summary>
    /// Handles a request to the skill as an asynchronous operation.
    /// </summary>
    /// <param name="request">The skill request.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the skill's response.
    /// </returns>
    public static async Task<SkillResponse> HandleAsync(SkillRequest request)
    {
        await EnsureInitialized();
        return await _function.HandlerAsync(request);
    }

    private static async Task EnsureInitialized()
    {
        if (!_initialized)
        {
            await _function.InitializeAsync();
            _initialized = true;
        }
    }
}
