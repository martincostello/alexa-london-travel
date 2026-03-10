// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Clients;

/// <summary>
/// A class representing a line's status. This class cannot be inherited.
/// </summary>
public sealed class LineStatus
{
    /// <summary>
    /// Gets or sets the status severity.
    /// </summary>
    [JsonPropertyName("statusSeverity")]
    public LineStatusSeverity StatusSeverity { get; set; }

    /// <summary>
    /// Gets or sets status reason.
    /// </summary>
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = default!;
}
