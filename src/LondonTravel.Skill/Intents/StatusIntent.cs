// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using MartinCostello.LondonTravel.Skill.Clients;
using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill.Intents;

/// <summary>
/// A class that handles the status intent. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="StatusIntent"/> class.
/// </remarks>
/// <param name="tflClient">The TfL API client to use.</param>
/// <param name="config">The skill configuration to use.</param>
internal sealed class StatusIntent(TflClient tflClient, SkillConfiguration config) : IIntent
{
    private static readonly CompositeFormat StatusIntentClosedFormat = CompositeFormat.Parse(Strings.StatusIntentClosedFormat);
    private static readonly CompositeFormat StatusIntentGoodServiceFormat = CompositeFormat.Parse(Strings.StatusIntentGoodServiceFormat);

    /// <inheritdoc />
    public async Task<SkillResponse> RespondAsync(Intent intent, Session session)
    {
        string? rawLineName = null;

        if (intent.Slots?.TryGetValue("LINE", out var slot) is true)
        {
            rawLineName = slot.Value;
        }

        string? id = Lines.MapNameToId(rawLineName);
        SkillResponseBuilder builder;

        if (string.IsNullOrEmpty(id))
        {
            builder = SkillResponseBuilder
                .Tell(Strings.StatusIntentUnknownLine)
                .WithReprompt();
        }
        else
        {
            var statuses = await GetStatusesAsync(id);
            var line = statuses[0];

            string cardTitle = Lines.ToCardTitle(line.Name ?? string.Empty);
            string text = GenerateResponse(line);

            builder = SkillResponseBuilder
                .Tell(text)
                .WithCard(cardTitle, text);
        }

        return builder.Build();
    }

    /// <summary>
    /// Generates the text to respond to the specified line's status response.
    /// </summary>
    /// <param name="line">The line to generate the response for.</param>
    /// <returns>
    /// The response for the specified line's status.
    /// </returns>
    internal static string GenerateResponse(Line line)
    {
        if (line.LineStatuses.Count == 1)
        {
            var lineStatus = line.LineStatuses[0];

            bool includeDetail = !ShouldStatusUseCustomResponse(lineStatus.StatusSeverity);

            return GenerateResponseForSingleStatus(
                line.Name,
                lineStatus,
                includeDetail);
        }
        else
        {
            return GenerateResponseForMultipleStatuses(line.Name, line.LineStatuses);
        }
    }

    /// <summary>
    /// Generates the text to respond for a multiple line statuses.
    /// </summary>
    /// <param name="name">The name of the line.</param>
    /// <param name="statuses">The statuses for the line.</param>
    /// <returns>
    /// The response for the specified line statuses.
    /// </returns>
    private static string GenerateResponseForMultipleStatuses(string name, IList<LineStatus> statuses)
    {
        // The descriptions appear to reference each other, so use the least severe's
        var leastSevere = statuses[0];

        for (int i = 1; i < statuses.Count; i++)
        {
            var status = statuses[i];

            if (status.StatusSeverity < leastSevere.StatusSeverity)
            {
                leastSevere = status;
            }
        }

        return GenerateResponseForSingleStatus(
          name,
          leastSevere,
          includeDetail: true);
    }

    /// <summary>
    /// Generates the text to respond for a single line status.
    /// </summary>
    /// <param name="name">The name of the line.</param>
    /// <param name="status">The status of the line.</param>
    /// <param name="includeDetail">Whether to include the detail in the response.</param>
    /// <returns>
    /// The response for the specified line status.
    /// </returns>
    private static string GenerateResponseForSingleStatus(string name, LineStatus status, bool includeDetail)
    {
        return includeDetail
            ? GenerateDetailedResponse(status)
            : GenerateSummaryResponse(name, status);
    }

    /// <summary>
    /// Generates the detailed text to respond for a single line status.
    /// </summary>
    /// <param name="status">The status for a line.</param>
    /// <returns>
    /// The response text for the specified line status.
    /// </returns>
    private static string GenerateDetailedResponse(LineStatus status)
    {
        string response = status.Reason;

        // Trim off the line name prefix, if present
        string delimiter = ": ";
        int index = response.IndexOf(delimiter, StringComparison.Ordinal);

        if (index > -1)
        {
            response = response[(index + delimiter.Length)..];
        }

        return response;
    }

    /// <summary>
    /// Generates the summary text to respond for a single line status.
    /// </summary>
    /// <param name="name">The name of the line.</param>
    /// <param name="status">The status for a line.</param>
    /// <returns>
    /// The response text for the specified line status.
    /// </returns>
    private static string GenerateSummaryResponse(string name, LineStatus status)
    {
        CompositeFormat format;

        if (status.StatusSeverity == LineStatusSeverity.ServiceClosed)
        {
            format = StatusIntentClosedFormat;
        }
        else
        {
            Debug.Assert(
                status.StatusSeverity is LineStatusSeverity.GoodService or LineStatusSeverity.NoIssues,
                $"'{status.StatusSeverity}' is not supported for a summary response.");

            format = StatusIntentGoodServiceFormat;
        }

        var culture = CultureInfo.CurrentCulture;
        string spokenName = Verbalizer.LineName(name);
        string statusText = string.Format(culture, format, spokenName);

        // Ensure the sentence starts with a capital letter
        char first = statusText[0];
        char upper = char.ToUpper(first, culture);

        return first == upper
            ? statusText
            : string.Concat(new ReadOnlySpan<char>(in upper), statusText.AsSpan(1));
    }

    /// <summary>
    /// Returns whether the specified status severity should use a custom response.
    /// </summary>
    /// <param name="statusSeverity">The status severity value.</param>
    /// <returns>
    /// <see langword="true"/> if the status should use a custom response; otherwise <see langword="false"/>.
    /// </returns>
    private static bool ShouldStatusUseCustomResponse(LineStatusSeverity statusSeverity)
    {
        return statusSeverity switch
        {
            LineStatusSeverity.GoodService => true,
            LineStatusSeverity.NoIssues => true,
            LineStatusSeverity.ServiceClosed => true,
            _ => false,
        };
    }

    private async Task<IList<Line>> GetStatusesAsync(string id)
    {
        return await tflClient.GetLineStatusAsync(
            id,
            config.TflApplicationId,
            config.TflApplicationKey);
    }
}
