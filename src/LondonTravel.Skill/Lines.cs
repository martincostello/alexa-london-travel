// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class containing helper methods for working with lines. This class cannot be inherited.
/// </summary>
internal static class Lines
{
    private static readonly CompositeFormat StatusIntentCardTitleFormat = CompositeFormat.Parse(Strings.StatusIntentCardTitleFormat);

    /// <summary>
    /// Returns whether the specified line name refers to the Docklands Light Railway.
    /// </summary>
    /// <param name="name">The name of the line as reported from the TfL API.</param>
    /// <returns>
    /// A boolean indicating whether the line is the DLR.
    /// </returns>
    public static bool IsDlr(string name)
    {
        return string.Equals(name, "dlr", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns whether the specified line name refers to the Elizabeth line.
    /// </summary>
    /// <param name="name">The name of the line as reported from the TfL API.</param>
    /// <returns>
    /// A boolean indicating whether the line is the Elizabeth line.
    /// </returns>
    public static bool IsElizabethLine(string name)
    {
        return name.Contains("elizabeth", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Maps the specified line name to a TfL API line Id.
    /// </summary>
    /// <param name="line">The line name.</param>
    /// <returns>
    /// The id for the specified line, if valid; otherwise <see langword="null"/>.
    /// </returns>
    public static string? MapNameToId(string? line)
    {
#pragma warning disable CA1308
        string normalized = (line ?? string.Empty).ToLowerInvariant();
#pragma warning restore CA1308

#pragma warning disable IDE0066
        switch (normalized)
        {
            case "bakerloo":
            case "central":
            case "circle":
            case "district":
            case "dlr":
            case "hammersmith-city":
            case "jubilee":
            case "liberty":
            case "lioness":
            case "london-overground":
            case "metropolitan":
            case "mildmay":
            case "northern":
            case "piccadilly":
            case "suffragette":
            case "tfl-rail":
            case "victoria":
            case "waterloo-city":
            case "weaver":
            case "windrush":
                return normalized;

            case "crossrail":
            case "elizabeth":
            case "elizabeth line":
            case "liz":
            case "liz line":
            case "lizzy":
            case "lizzy line":
                return "elizabeth";

            case "met":
                return "metropolitan";

            case "docklands":
            case "docklands light rail":
            case "docklands light railway":
            case "docklands rail":
            case "docklands railway":
                return "dlr";

            case "hammersmith":
            case "hammersmith and city":
            case "hammersmith & city":
                return "hammersmith-city";

            case "city":
            case "waterloo":
            case "waterloo and city":
            case "waterloo & city":
                return "waterloo-city";

            case "london overground":
            case "overground":
            default:
                return null;
        }
#pragma warning restore IDE0066
    }

    /// <summary>
    /// Returns the title to use for a status card for the specified line name.
    /// </summary>
    /// <param name="name">The name of the line as reported from the TfL API.</param>
    /// <returns>
    /// The title to use for the status card.
    /// </returns>
    public static string ToCardTitle(string name)
    {
        bool isNameWithoutLine =
            IsDlr(name) ||
            IsElizabethLine(name);

        string suffix = isNameWithoutLine ? Strings.LineSuffixUpper : string.Empty;
        return string.Format(CultureInfo.CurrentCulture, StatusIntentCardTitleFormat, name, suffix);
    }
}
