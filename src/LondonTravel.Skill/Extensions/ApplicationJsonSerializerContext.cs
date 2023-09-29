// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using MartinCostello.LondonTravel.Skill.Clients;

namespace MartinCostello.LondonTravel.Skill.Extensions;

#pragma warning disable SA1601

[ExcludeFromCodeCoverage]
[JsonSerializable(typeof(IList<Line>))]
[JsonSerializable(typeof(IList<ServiceDisruption>))]
[JsonSerializable(typeof(SkillUserPreferences))]
internal sealed partial class ApplicationJsonSerializerContext : JsonSerializerContext
{
}
