// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Clients;

/// <summary>
/// A class representing a service disruption. This class cannot be inherited.
/// </summary>
internal sealed class ServiceDisruption
{
    /// <summary>
    /// Gets or sets the description of the disruption.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }
}
