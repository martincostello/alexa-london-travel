// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace LondonTravel.Skill.EndToEndTests;

/// <summary>
/// See https://docs.aws.amazon.com/lambda/latest/dg/telemetry-schema-reference.html#ReportMetrics.
/// </summary>
internal sealed class PlatformReportMetrics
{
    [JsonPropertyName("durationMs")]
    public double DurationMs { get; set; }

    [JsonPropertyName("billedDurationMs")]
    public int BilledDurationMs { get; set; }

    [JsonPropertyName("memorySizeMB")]
    public int MemorySizeMB { get; set; }

    [JsonPropertyName("maxMemoryUsedMB")]
    public int MaxMemoryUsedMB { get; set; }

    [JsonPropertyName("initDurationMs")]
    public double? InitDurationMs { get; set; }
}
