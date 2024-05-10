// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.EndToEndTests;

/// <summary>
/// See https://docs.aws.amazon.com/lambda/latest/dg/telemetry-schema-reference.html#platform-report.
/// </summary>
internal sealed class PlatformReport
{
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = default!;

    [JsonPropertyName("metrics")]
    public PlatformReportMetrics Metrics { get; set; } = default!;

    [JsonPropertyName("tracing")]
    public PlatformTraceContext? Tracing { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;
}
