// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Skill.Clients;
using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill.Intents;

/// <summary>
/// A class that handles the disruption intent. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DisruptionIntent"/> class.
/// </remarks>
/// <param name="tflClient">The TfL API client to use.</param>
/// <param name="config">The skill configuration to use.</param>
internal sealed class DisruptionIntent(TflClient tflClient, SkillConfiguration config) : IIntent
{
    /// <summary>
    /// The supported modes of transport. This field is read-only.
    /// </summary>
    private static readonly string SupportedModes = string.Join(',', "dlr", "elizabeth-line", "overground", "tube");

    /// <inheritdoc />
    public async Task<SkillResponse> RespondAsync(Intent intent, Session session)
    {
        var sentences = await GetRawDisruptionsAsync();
        string cardContent;

        if (sentences.Count > 0)
        {
            cardContent = string.Join('\n', sentences);
            sentences.Add(Strings.DisruptionIntentGoodServiceOnOtherLines);
        }
        else
        {
            string text = Strings.DisruptionIntentNoDisruption;

            sentences.Add(text);
            cardContent = text;
        }

        return SkillResponseBuilder
            .Tell(sentences)
            .WithCard(Strings.DisruptionIntentCardTitle, cardContent)
            .Build();
    }

    private async Task<IList<ServiceDisruption>> GetDisruptionAsync()
    {
        return await tflClient.GetDisruptionAsync(
            SupportedModes,
            config.TflApplicationId,
            config.TflApplicationKey);
    }

    private async Task<List<string>> GetRawDisruptionsAsync()
    {
        var disruptions = await GetDisruptionAsync();

        var descriptions = new List<string>(disruptions.Count);

        foreach (var disruption in disruptions)
        {
            descriptions.Add(disruption.Description);
        }

        // Deduplicate any status descriptions. For example, if a tube line
        // has a planned closure and severe delays, the message will appear twice.
        var distinct = descriptions
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase);

        return [..distinct];
    }
}
