// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Clients;

/// <summary>
/// A class representing a line. This class cannot be inherited.
/// </summary>
public sealed class Line
{
    /// <summary>
    /// Gets or sets the line's name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the statuses for the line.
    /// </summary>
    [JsonPropertyName("lineStatuses")]
    public IList<LineStatus> LineStatuses { get; set; }
}
