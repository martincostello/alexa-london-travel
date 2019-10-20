// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Clients
{
    /// <summary>
    /// A class representing a user's preferences for the skill. This class cannot be inherited.
    /// </summary>
    internal sealed class SkillUserPreferences
    {
        /// <summary>
        /// Gets or sets the user's favorite lines.
        /// </summary>
        [JsonPropertyName("favoriteLines")]
        public IList<string> FavoriteLines { get; set; }

        /// <summary>
        /// Gets or sets the user's Id.
        /// </summary>
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
    }
}
