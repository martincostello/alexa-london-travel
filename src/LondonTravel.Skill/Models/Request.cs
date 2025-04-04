// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

[JsonDerivedType(typeof(IntentRequest), RequestTypes.Intent)]
[JsonDerivedType(typeof(LaunchRequest), RequestTypes.Launch)]
[JsonDerivedType(typeof(SessionEndedRequest), RequestTypes.SessionEnded)]
[JsonDerivedType(typeof(SystemExceptionRequest), RequestTypes.SystemException)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
public abstract class Request
{
    [JsonIgnore]
    public abstract string Type { get; }

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = default!;

    [JsonPropertyName("locale")]
    public string Locale { get; set; } = default!;

    [JsonConverter(typeof(MixedDateTimeConverter))]
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
