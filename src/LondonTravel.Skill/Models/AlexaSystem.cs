// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

public sealed class AlexaSystem
{
    [JsonPropertyName("apiAccessToken")]
    public string ApiAccessToken { get; set; } = default!;

    [JsonPropertyName("apiEndpoint")]
    public string ApiEndpoint { get; set; } = default!;

    [JsonPropertyName("application")]
    public Application Application { get; set; } = default!;

    [JsonPropertyName("user")]
    public User User { get; set; } = default!;

    [JsonPropertyName("device")]
    public Device Device { get; set; } = default!;
}
