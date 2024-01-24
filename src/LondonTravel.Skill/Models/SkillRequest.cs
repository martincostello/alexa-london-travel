// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

public sealed class SkillRequest
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = default!;

    [JsonPropertyName("session")]
    public Session Session { get; set; } = default!;

    [JsonPropertyName("context")]
    public Context Context { get; set; } = default!;

    [JsonPropertyName("request")]
    public Request Request { get; set; } = default!;
}
