// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

#pragma warning disable CA1724
public sealed class Context
#pragma warning restore CA1724
{
    [JsonPropertyName("System")]
    public AlexaSystem System { get; set; } = default!;
}
