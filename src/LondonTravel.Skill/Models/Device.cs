// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

public sealed class Device
{
    [JsonPropertyName("deviceId")]
    public string? DeviceId { get; set; }

    [JsonPropertyName("supportedInterfaces")]
    public Dictionary<string, object>? SupportedInterfaces { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("persistentEndpointId")]
    public string? PersistentEndpointId { get; set; }
}
