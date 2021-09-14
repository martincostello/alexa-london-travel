// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using Alexa.NET.Response;
using MartinCostello.LondonTravel.Skill.Clients;

namespace MartinCostello.LondonTravel.Skill.Intents;

/// <summary>
/// A class that handles the disruption intent. This class cannot be inherited.
/// </summary>
internal sealed class DisruptionIntent : IIntent
{
    /// <summary>
    /// The supported modes of transport. This field is read-only.
    /// </summary>
    private static readonly string SupportedModes = string.Join(",", "dlr", "overground", "tube", "tflrail");

    /// <summary>
    /// Initializes a new instance of the <see cref="DisruptionIntent"/> class.
    /// </summary>
    /// <param name="tflClient">The TfL API client to use.</param>
    /// <param name="config">The skill configuration to use.</param>
    public DisruptionIntent(ITflClient tflClient, SkillConfiguration config)
    {
        Config = config;
        TflClient = tflClient;
    }

    /// <summary>
    /// Gets the skill configuration.
    /// </summary>
    private SkillConfiguration Config { get; }

    /// <summary>
    /// Gets the TfL API client.
    /// </summary>
    private ITflClient TflClient { get; }

    /// <inheritdoc />
    public async Task<SkillResponse> RespondAsync(Intent intent, Session session)
    {
        ICollection<string> sentences = await GetRawDisruptionsAsync();
        string cardContent;

        if (sentences.Count > 0)
        {
            cardContent = string.Join("\n", sentences);
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

    private async Task<ICollection<ServiceDisruption>> GetDisruptionAsync()
    {
        return await TflClient.GetDisruptionAsync(
            SupportedModes,
            Config.TflApplicationId,
            Config.TflApplicationKey);
    }

    private async Task<IList<string>> GetRawDisruptionsAsync()
    {
        ICollection<ServiceDisruption> disruptions = await GetDisruptionAsync();

        var descriptions = new List<string>();

        foreach (var disruption in disruptions)
        {
            descriptions.Add(disruption.Description);
        }

        // Deduplicate any status descriptions. For example, if a tube line
        // has a planned closure and severe delays, the message will appear twice.
        return descriptions
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy((p) => p, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
