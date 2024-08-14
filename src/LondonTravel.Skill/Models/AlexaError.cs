// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

public sealed class AlexaError
{
    [JsonConverter(typeof(StringEnumConverter<AlexaErrorType>))]
    [JsonPropertyName("type")]
    public AlexaErrorType Type { get; set; } = default!;

    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;
}
