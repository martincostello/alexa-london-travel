// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

[JsonDerivedType(typeof(LinkAccountCard), CardTypes.LinkAccount)]
[JsonDerivedType(typeof(StandardCard), CardTypes.Standard)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
public abstract class Card
{
    [JsonIgnore]
    public abstract string Type { get; }
}
