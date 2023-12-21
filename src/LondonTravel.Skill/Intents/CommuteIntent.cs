// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using MartinCostello.LondonTravel.Skill.Clients;
using MartinCostello.LondonTravel.Skill.Models;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill.Intents;

/// <summary>
/// A class that handles the commute intent. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CommuteIntent"/> class.
/// </remarks>
/// <param name="skillClient">The skill client to use.</param>
/// <param name="tflClient">The TfL API client to use.</param>
/// <param name="contextAccessor">The AWS Lambda context accessor to use.</param>
/// <param name="config">The skill configuration to use.</param>
/// <param name="logger">The logger to use.</param>
internal sealed class CommuteIntent(
    SkillClient skillClient,
    TflClient tflClient,
    SkillConfiguration config,
    ILogger<CommuteIntent> logger) : IIntent
{
    private static readonly CompositeFormat CommuteIntentPrefixFormat = CompositeFormat.Parse(Strings.CommuteIntentPrefixFormat);

    /// <inheritdoc />
    public async Task<SkillResponse> RespondAsync(Intent intent, Session session)
    {
        string accessToken = session?.User?.AccessToken;

        if (string.IsNullOrEmpty(accessToken))
        {
            return NotLinked(session);
        }

        ICollection<string> favoriteLines = await GetFavoriteLinesAsync(accessToken);

        if (favoriteLines == null)
        {
            return Unauthorized(session);
        }
        else if (favoriteLines.Count < 1)
        {
            return NoFavorites(session);
        }
        else
        {
            return await CommuteAsync(favoriteLines);
        }
    }

    private async Task<SkillResponse> CommuteAsync(ICollection<string> favoriteLines)
    {
        IList<Line> lines = await GetStatusesAsync(string.Join(',', favoriteLines));

        var paragraphs = new List<string>(lines.Count);

        bool hasMultipleStatuses = lines.Count > 1;

        foreach (Line line in lines.OrderBy((p) => p.Name, StringComparer.Ordinal))
        {
            string text = StatusIntent.GenerateResponse(new[] { line });

            if (hasMultipleStatuses)
            {
                string displayName = Verbalizer.LineName(line.Name, asTitleCase: true);
                text = string.Format(CultureInfo.CurrentCulture, CommuteIntentPrefixFormat, displayName, text);
            }

            paragraphs.Add(text);
        }

        return SkillResponseBuilder
            .Tell(paragraphs)
            .WithCard(Strings.CommuteIntentCardTitle, string.Join('\n', paragraphs))
            .Build();
    }

    private async Task<ICollection<string>> GetFavoriteLinesAsync(string accessToken)
    {
        try
        {
            SkillUserPreferences preferences = await skillClient.GetPreferencesAsync(accessToken);
            return preferences.FavoriteLines ?? [];
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }
    }

    private async Task<IList<Line>> GetStatusesAsync(string ids)
    {
        return await tflClient.GetLineStatusAsync(
            ids,
            config.TflApplicationId,
            config.TflApplicationKey);
    }

    private SkillResponse NoFavorites(Session session)
    {
        Log.NoLinePreferences(logger, session.User.UserId);

        string text = Strings.CommuteIntentNoFavorites;

        return SkillResponseBuilder
            .Tell(text)
            .WithCard(Strings.CommuteIntentCardTitle, text)
            .Build();
    }

    private SkillResponse NotLinked(Session session)
    {
        Log.AccountNotLinked(logger, session.User?.UserId);

        return SkillResponseBuilder
            .Tell(Strings.CommuteIntentAccountNotLinked)
            .LinkAccount()
            .Build();
    }

    private SkillResponse Unauthorized(Session session)
    {
        Log.InvalidAccessToken(logger, session.User.UserId, session.SessionId);

        return SkillResponseBuilder
            .Tell(Strings.CommuteIntentInvalidToken)
            .LinkAccount()
            .Build();
    }
}
