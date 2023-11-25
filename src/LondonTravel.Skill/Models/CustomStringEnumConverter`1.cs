// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

internal sealed class CustomStringEnumConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>()
    : JsonStringEnumConverter<TEnum>(namingPolicy: ResolveNamingPolicy())
    where TEnum : struct, Enum
{
    private static EnumMemberNamingPolicy ResolveNamingPolicy()
    {
        var map = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select((p) => (p.Name, AttributeName: p.GetCustomAttribute<EnumMemberAttribute>()?.Value))
            .Where((p) => p.AttributeName is not null)
            .ToDictionary();

        return map.Count > 0 ? new EnumMemberNamingPolicy(map) : null;
    }

    private sealed class EnumMemberNamingPolicy(Dictionary<string, string> map) : JsonNamingPolicy
    {
        public override string ConvertName(string name)
            => map.TryGetValue(name, out string overriden) ? overriden : name;
    }
}
