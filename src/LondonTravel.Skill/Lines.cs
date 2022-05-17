// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class containing helper methods for working with lines. This class cannot be inherited.
/// </summary>
internal static class Lines
{
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
        return string.Equals(name, "elizabeth", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns whether the specified line name refers to the London Overground.
    /// </summary>
    /// <param name="name">The name of the line as reported from the TfL API.</param>
    /// <returns>
    /// A boolean indicating whether the line is the London Overground.
    /// </returns>
    public static bool IsOverground(string name)
    {
        return name.Contains("overground", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns whether the specified line name refers to TfL Rail.
    /// </summary>
    /// <param name="name">The name of the line as reported from the TfL API.</param>
    /// <returns>
    /// A boolean indicating whether the line is TfL Rail.
    /// </returns>
    public static bool IsTfLRail(string name)
    {
        return name.Contains("tfl", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Maps the specified line name to a TfL API line Id.
    /// </summary>
    /// <param name="line">The line name.</param>
    /// <returns>
    /// The id for the specified line, if valid; otherwise <see langword="null"/>.
    /// </returns>
    public static string MapNameToId(string line)
    {
#pragma warning disable CA1308
        string normalized = (line ?? string.Empty).ToLowerInvariant();
#pragma warning restore CA1308

        switch (normalized)
        {
            case "bakerloo":
            case "central":
            case "circle":
            case "district":
            case "dlr":
            case "hammersmith-city":
            case "jubilee":
            case "london-overground":
            case "metropolitan":
            case "northern":
            case "piccadilly":
            case "tfl-rail":
            case "victoria":
            case "waterloo-city":
                return normalized;

            case "crossrail":
            case "elizabeth":
            case "elizabeth line":
                return "elizabeth";

            case "london overground":
            case "overground":
                return "london-overground";

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

            case "tfl rail":
                return "tfl-rail";

            default:
                return null;
        }
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
        string suffix;

        if (IsDlr(name) ||
            IsElizabethLine(name) ||
            IsOverground(name) ||
            IsTfLRail(name))
        {
            suffix = string.Empty;
        }
        else
        {
            suffix = Strings.LineSuffixUpper;
        }

        return string.Format(CultureInfo.CurrentCulture, Strings.StatusIntentCardTitleFormat, name, suffix);
    }
}
