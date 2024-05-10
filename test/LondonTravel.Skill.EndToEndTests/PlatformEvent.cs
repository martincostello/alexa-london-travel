// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace LondonTravel.Skill.EndToEndTests;

/// <summary>
/// See https://docs.aws.amazon.com/lambda/latest/dg/telemetry-schema-reference.html#telemetry-api-events.
/// </summary>
[JsonDerivedType(typeof(PlatformInitEvent), PlatformEventType.Initialize)]
[JsonDerivedType(typeof(PlatformReportEvent), PlatformEventType.Report)]
[JsonDerivedType(typeof(PlatformStartEvent), PlatformEventType.Start)]
[JsonDerivedType(typeof(PlatformEmptyEvent), "platform.logsDropped")]
[JsonDerivedType(typeof(PlatformEmptyEvent), "platform.initReport")]
[JsonDerivedType(typeof(PlatformEmptyEvent), "platform.initRuntimeDone")]
[JsonDerivedType(typeof(PlatformEmptyEvent), "platform.restoreReport")]
[JsonDerivedType(typeof(PlatformEmptyEvent), "platform.restoreRuntimeDone")]
[JsonDerivedType(typeof(PlatformEmptyEvent), "platform.restoreStart")]
[JsonDerivedType(typeof(PlatformEmptyEvent), "platform.runtimeDone")]
[JsonDerivedType(typeof(PlatformEmptyEvent), "platform.telemetrySubscription")]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
internal abstract class PlatformEvent
{
    [JsonPropertyName("time")]
    public DateTime Timestamp { get; set; }

    [JsonIgnore]
    public abstract string Type { get; }
}
