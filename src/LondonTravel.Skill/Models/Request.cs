// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

[JsonDerivedType(typeof(IntentRequest), "IntentRequest")]
[JsonDerivedType(typeof(LaunchRequest), "LaunchRequest")]
[JsonDerivedType(typeof(SessionEndedRequest), "SessionEndedRequest")]
[JsonDerivedType(typeof(SystemExceptionRequest), "System.ExceptionEncountered")]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
public class Request
{
    [JsonIgnore]
    public virtual string Type { get; } = default!;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = default!;

    [JsonPropertyName("locale")]
    public string Locale { get; set; } = default!;

    [JsonConverter(typeof(MixedDateTimeConverter))]
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
