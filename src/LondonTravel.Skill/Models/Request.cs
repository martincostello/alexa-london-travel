// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

public sealed class Request : IRequest, IIntentRequest, ISessionEndedRequest, ISystemExceptionRequest
{
    [JsonPropertyName("type")]
    [JsonRequired]
    public string Type { get; set; } = default!;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = default!;

    [JsonPropertyName("locale")]
    public string Locale { get; set; } = default!;

    [JsonConverter(typeof(MixedDateTimeConverter))]
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    //// Properties for "IntentRequest"

    [JsonPropertyName("dialogState")]
    public string DialogState { get; set; } = default!;

    [JsonPropertyName("intent")]
    public Intent Intent { get; set; } = default!;

    //// Properties for "SessionEndedRequest"

    [JsonConverter(typeof(CustomStringEnumConverter<Reason>))]
    [JsonPropertyName("reason")]
    public Reason Reason { get; set; }

    //// Properties for "System.ExceptionEncountered"

    [JsonPropertyName("cause")]
    public AlexaErrorCause ErrorCause { get; set; } = default!;

    //// Properties for "SessionEndedRequest" and "System.ExceptionEncountered"

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("error")]
    public AlexaError Error { get; set; } = default!;
}
