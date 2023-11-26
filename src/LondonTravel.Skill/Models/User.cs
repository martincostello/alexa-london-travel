// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

public sealed class User
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }
}
