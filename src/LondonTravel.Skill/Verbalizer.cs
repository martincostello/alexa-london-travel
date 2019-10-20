// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace MartinCostello.LondonTravel.Skill
{
    /// <summary>
    /// A class that improves a text string for speech purposes. This class cannot be inherited.
    /// </summary>
    internal static class Verbalizer
    {
        /// <summary>
        /// Returns the spoken version of the specified line name.
        /// </summary>
        /// <param name="name">The name of the line as reported from the TfL API.</param>
        /// <param name="asTitleCase">Whether to format in title case.</param>
        /// <returns>
        /// The spoken name of the line.
        /// </returns>
        internal static string LineName(string name, bool asTitleCase = false)
        {
            string prefix = string.Empty;
            string suffix = string.Empty;

            string spokenName;

            if (Lines.IsDlr(name))
            {
                prefix = "the ";
                spokenName = Verbalize("DLR");
            }
            else if (Lines.IsOverground(name))
            {
                spokenName = name;
            }
            else if (Lines.IsTfLRail(name))
            {
                spokenName = Verbalize("TfL Rail");
            }
            else
            {
                prefix = "the ";
                spokenName = name;
                suffix = asTitleCase ? " Line" : " line";
            }

            var culture = CultureInfo.CurrentUICulture;

            if (asTitleCase)
            {
                return string.Format(culture, "{0}{1}", spokenName, suffix);
            }
            else
            {
                return string.Format(culture, "{0}{1}{2}", prefix, spokenName, suffix);
            }
        }

        /// <summary>
        /// Returns a string which better represents the specified text when spoken.
        /// </summary>
        /// <param name="text">The text to verbalize.</param>
        /// <returns>
        /// The representation of the text that is enhanced for being spoken aloud by Alexa.
        /// </returns>
        internal static string Verbalize(string text)
        {
            return text
              .Replace("DLR", "D.L.R.", StringComparison.Ordinal)
              .Replace("e/b", "eastbound", StringComparison.Ordinal)
              .Replace("n/b", "northbound", StringComparison.Ordinal)
              .Replace("s/b", "southbound", StringComparison.Ordinal)
              .Replace("w/b", "westbound", StringComparison.Ordinal)
              .Replace("SWT", "South West Trains", StringComparison.Ordinal)
              .Replace("tfl", "T.F.L.", StringComparison.Ordinal)
              .Replace("TFL", "T.F.L.", StringComparison.Ordinal)
              .Replace("TfL", "T.F.L.", StringComparison.Ordinal)
              .Replace(" & ", " and ", StringComparison.Ordinal);
        }
    }
}
