// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.EndToEndTests;

/// <summary>
/// See https://docs.aws.amazon.com/lambda/latest/dg/telemetry-schema-reference.html#platform-runtimeDone.
/// </summary>
internal sealed class PlatformRuntimeDoneEvent : PlatformEvent
{
    public override string Type => PlatformEventType.RuntimeDone;

    [JsonPropertyName("record")]
    public PlatformReport Record { get; set; } = default!;
}
