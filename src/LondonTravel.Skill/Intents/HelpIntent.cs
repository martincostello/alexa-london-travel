// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill.Intents;

/// <summary>
/// A class that handles the <c>AMAZON.HelpIntent</c> intent. This class cannot be inherited.
/// </summary>
internal sealed class HelpIntent : IIntent
{
    private static readonly string[] Paragraphs =
    [
        Strings.HelpIntentParagraph1,
        Strings.HelpIntentParagraph2,
        Strings.HelpIntentParagraph3,
        Strings.HelpIntentParagraph4,
        Strings.HelpIntentParagraph5,
    ];

    /// <inheritdoc />
    public Task<SkillResponse> RespondAsync(Intent intent, Session session)
    {
        var result = SkillResponseBuilder
            .Tell(Paragraphs)
            .ShouldNotEndSession()
            .Build();

        return Task.FromResult(result);
    }
}
