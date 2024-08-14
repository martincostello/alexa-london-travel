// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

public sealed class SessionEndedRequest : Request
{
    [JsonIgnore]
    public override string Type => "SessionEndedRequest";

    [JsonPropertyName("reason")]
    public Reason Reason { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("error")]
    public AlexaError Error { get; set; } = default!;
}
