// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace LondonTravel.Skill.EndToEndTests;

/// <summary>
/// See https://docs.aws.amazon.com/lambda/latest/dg/telemetry-schema-reference.html#telemetry-api-events.
/// </summary>
//// TODO Make polymorphic for .NET 9 with AllowOutOfOrderMetadataProperties = true
////[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
internal sealed class PlatformEvent
{
    [JsonPropertyName("time")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;

    [JsonPropertyName("record")]
    public PlatformLogRecord? Record { get; set; }
}
