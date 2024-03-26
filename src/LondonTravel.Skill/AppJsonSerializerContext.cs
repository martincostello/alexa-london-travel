// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using MartinCostello.LondonTravel.Skill.Clients;
using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class representing the <see cref="JsonSerializerContext"/> to use for the application.
/// </summary>
[ExcludeFromCodeCoverage]
[JsonSerializable(typeof(IList<Line>))]
[JsonSerializable(typeof(IList<ServiceDisruption>))]
[JsonSerializable(typeof(LinkAccountCard))]
[JsonSerializable(typeof(SkillRequest))]
[JsonSerializable(typeof(SkillResponse))]
[JsonSerializable(typeof(SkillUserPreferences))]
[JsonSerializable(typeof(StandardCard))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public sealed partial class AppJsonSerializerContext : JsonSerializerContext;
