// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using MartinCostello.LondonTravel.Skill.Clients;

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class representing the <see cref="JsonSerializerContext"/> to use for the application.
/// </summary>
[JsonSerializable(typeof(IList<Line>))]
[JsonSerializable(typeof(IList<ServiceDisruption>))]
[JsonSerializable(typeof(SkillUserPreferences))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class AppJsonSerializerContext : JsonSerializerContext
{
}
