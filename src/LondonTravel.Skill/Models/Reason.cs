// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

public enum Reason
{
    [JsonStringEnumMemberName("USER_INITIATED")]
    UserInitiated = 0,

    [JsonStringEnumMemberName("ERROR")]
    Error,

    [JsonStringEnumMemberName("EXCEEDED_MAX_REPROMPTS")]
    ExceededMaxReprompts,
}
