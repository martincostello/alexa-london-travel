// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace LondonTravel.Skill.EndToEndTests;

/// <summary>
/// See https://docs.aws.amazon.com/lambda/latest/dg/telemetry-schema-reference.html#platform-report.
/// </summary>
internal sealed class PlatformReportEvent : PlatformEvent
{
    public override string Type => PlatformEventType.Report;

    [JsonPropertyName("record")]
    public PlatformReport Record { get; set; } = default!;
}