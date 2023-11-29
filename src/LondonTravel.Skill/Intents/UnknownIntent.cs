// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Skill.Models;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill.Intents;

/// <summary>
/// A class that handles an unknown intent. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UnknownIntent"/> class.
/// </remarks>
/// <param name="logger">The logger to use.</param>
internal sealed class UnknownIntent(ILogger<UnknownIntent> logger) : IIntent
{
    /// <inheritdoc />
    public Task<SkillResponse> RespondAsync(Intent intent, Session session)
    {
        Log.UnknownIntent(logger, intent.Name, session.SessionId);

        var response = SkillResponseBuilder
            .Tell(Strings.UnknownCommand)
            .Build();

        return Task.FromResult(response);
    }
}
