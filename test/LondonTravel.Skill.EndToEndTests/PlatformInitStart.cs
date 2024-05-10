// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace LondonTravel.Skill.EndToEndTests;

/// <summary>
/// See https://docs.aws.amazon.com/lambda/latest/dg/telemetry-schema-reference.html#platform-initStart.
/// </summary>
internal sealed class PlatformInitStart
{
    [JsonPropertyName("initializationType")]
    public string InitializationType { get; set; } = default!;

    [JsonPropertyName("phase")]
    public string Phase { get; set; } = default!;

    [JsonPropertyName("runtimeVersion")]
    public string? RuntimeVersion { get; set; }

    [JsonPropertyName("runtimeVersionArn")]
    public string? RuntimeVersionArn { get; set; }

    [JsonPropertyName("functionName")]
    public string FunctionName { get; set; } = default!;

    [JsonPropertyName("functionVersion")]
    public string FunctionVersion { get; set; } = default!;
}
