// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using MartinCostello.LondonTravel.Skill.Clients;
using Refit;

namespace MartinCostello.LondonTravel.Skill.Intents
{
    /// <summary>
    /// A class that handles the commute intent. This class cannot be inherited.
    /// </summary>
    internal sealed class CommuteIntent : IIntent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommuteIntent"/> class.
        /// </summary>
        /// <param name="skillClient">The skill client to use.</param>
        /// <param name="tflClient">The TfL API client to use.</param>
        /// <param name="contextAccessor">The AWS Lambda context accessor to use.</param>
        /// <param name="config">The skill configuration to use.</param>
        public CommuteIntent(
            ISkillClient skillClient,
            ITflClient tflClient,
            LambdaContextAccessor contextAccessor,
            SkillConfiguration config)
        {
            Config = config;
            ContextAccessor = contextAccessor;
            SkillClient = skillClient;
            TflClient = tflClient;
        }

        /// <summary>
        /// Gets the skill configuration.
        /// </summary>
        private SkillConfiguration Config { get; }

        /// <summary>
        /// Gets the Lambda context accessor.
        /// </summary>
        private LambdaContextAccessor ContextAccessor { get; }

        /// <summary>
        /// Gets the skill client.
        /// </summary>
        private ISkillClient SkillClient { get; }

        /// <summary>
        /// Gets the TfL API client.
        /// </summary>
        private ITflClient TflClient { get; }

        /// <inheritdoc />
        public async Task<SkillResponse> RespondAsync(Intent intent, Session session)
        {
            string accessToken = session?.User?.AccessToken;

            if (string.IsNullOrEmpty(accessToken))
            {
                return NotLinked(session);
            }

            ICollection<string> favoriteLines = await GetFavoriteLinesAsync(session, accessToken);

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
            IList<Line> lines = await GetStatusesAsync(string.Join(",", favoriteLines));

            var paragraphs = new List<string>();

            bool hasMultipleStatuses = lines.Count > 1;

            foreach (Line line in lines.OrderBy((p) => p.Name, StringComparer.Ordinal))
            {
                string text = StatusIntent.GenerateResponse(new[] { line });

                if (hasMultipleStatuses)
                {
                    string displayName = Verbalizer.LineName(line.Name, asTitleCase: true);
                    text = string.Format(CultureInfo.CurrentCulture, Strings.CommuteIntentPrefixFormat, displayName, text);
                }

                paragraphs.Add(text);
            }

            return SkillResponseBuilder
                .Tell(paragraphs)
                .WithCard(Strings.CommuteIntentCardTitle, string.Join("\n", paragraphs))
                .Build();
        }

        private async Task<ICollection<string>> GetFavoriteLinesAsync(Session session, string accessToken)
        {
            try
            {
                SkillUserPreferences preferences = await SkillClient.GetPreferencesAsync($"Bearer {accessToken}");
                var favouriteLines = preferences.FavoriteLines ?? Array.Empty<string>();

                // TODO Remove once supported by the TfL API (see https://github.com/martincostello/alexa-london-travel/issues/54)
                favouriteLines.Remove("elizabeth");

                return favouriteLines;
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                ContextAccessor.LambdaContext.Logger.LogLine(
                    $"Access token is invalid for user Id '{session.User.UserId}' and session id '{session.SessionId}'.");

                return null;
            }
        }

        private async Task<IList<Line>> GetStatusesAsync(string ids)
        {
            return await TflClient.GetLineStatusAsync(
                ids,
                Config.TflApplicationId,
                Config.TflApplicationKey);
        }

        private SkillResponse NoFavorites(Session session)
        {
            ContextAccessor.LambdaContext.Logger.LogLine(
                $"User with Id '{session.User.UserId}' has set no line preferences.");

            string text = Strings.CommuteIntentNoFavorites;

            return SkillResponseBuilder
                .Tell(text)
                .WithCard(Strings.CommuteIntentCardTitle, text)
                .Build();
        }

        private SkillResponse NotLinked(Session session)
        {
            ContextAccessor.LambdaContext.Logger.LogLine(
                $"User with Id '{session.User?.UserId}' has not linked account.");

            return SkillResponseBuilder
                .Tell(Strings.CommuteIntentAccountNotLinked)
                .LinkAccount()
                .Build();
        }

        private SkillResponse Unauthorized(Session session)
        {
            ContextAccessor.LambdaContext.Logger.LogLine(
                $"User with Id '{session.User.UserId}' has an invalid access token.");

            return SkillResponseBuilder
                .Tell(Strings.CommuteIntentInvalidToken)
                .LinkAccount()
                .Build();
        }
    }
}
