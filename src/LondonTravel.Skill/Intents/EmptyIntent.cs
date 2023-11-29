// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill.Intents;

/// <summary>
/// A class that handles an intent with no response. This class cannot be inherited.
/// </summary>
internal sealed class EmptyIntent : IIntent
{
    /// <inheritdoc />
    public Task<SkillResponse> RespondAsync(Intent intent, Session session)
    {
        SkillResponse response = new()
        {
            Response = new() { ShouldEndSession = true },
        };
        return Task.FromResult(response);
    }
}
