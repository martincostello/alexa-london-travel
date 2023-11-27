// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Runtime.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

public enum Reason
{
    [EnumMember(Value = "USER_INITIATED")]
    UserInitiated = 0,

    [EnumMember(Value = "ERROR")]
    Error,

    [EnumMember(Value = "EXCEEDED_MAX_REPROMPTS")]
    ExceededMaxReprompts,
}
