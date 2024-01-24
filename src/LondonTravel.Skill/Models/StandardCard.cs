// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

public sealed class StandardCard : Card
{
    [JsonPropertyName("type")]
    [JsonRequired]
    public override string Type { get; set; } = "Standard";

    [JsonPropertyName("title")]
    [JsonRequired]
    public string Title { get; set; } = default!;

    [JsonPropertyName("text")]
    [JsonRequired]
    public string Content { get; set; } = default!;
}
