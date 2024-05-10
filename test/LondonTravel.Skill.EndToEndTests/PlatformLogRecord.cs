// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace LondonTravel.Skill.EndToEndTests;

/// <summary>
/// See https://docs.aws.amazon.com/lambda/latest/dg/telemetry-schema-reference.html#telemetry-api-events.
/// </summary>
internal sealed class PlatformLogRecord
{
    //// platform.initStart

    [JsonPropertyName("initializationType")]
    public string? InitializationType { get; set; }

    [JsonPropertyName("phase")]
    public string? Phase { get; set; }

    [JsonPropertyName("runtimeVersion")]
    public string? RuntimeVersion { get; set; }

    [JsonPropertyName("runtimeVersionArn")]
    public string? RuntimeVersionArn { get; set; }

    [JsonPropertyName("functionName")]
    public string? FunctionName { get; set; }

    [JsonPropertyName("functionVersion")]
    public string? FunctionVersion { get; set; }

    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; } // platform.start and platform.report

    [JsonPropertyName("version")]
    public string? Version { get; set; } // platform.start

    [JsonPropertyName("metrics")]
    public PlatformReportMetrics? Metrics { get; set; } // platform.report

    [JsonPropertyName("tracing")]
    public PlatformTraceContext? Tracing { get; set; } // platform.start and platform.report

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
