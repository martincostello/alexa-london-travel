// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill;

#pragma warning disable JSON002

public static class MixedDateTimeConverterTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    [Fact]
    public static void Converter_Converts_Json_String()
    {
        // Arrange
        string json =
            """
            {
                "Value": "2023-10-01T12:00:00Z"
            }
            """;

        // Act
        var actual = JsonSerializer.Deserialize<Model>(json, Options);

        // Assert
        actual.ShouldNotBeNull();
        actual.Value.ShouldBe(new(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public static void Converter_Converts_Json_Number()
    {
        // Arrange
        string json =
            """
            {
                "Value": 1753609472466
            }
            """;

        // Act
        var actual = JsonSerializer.Deserialize<Model>(json, Options);

        // Assert
        actual.ShouldNotBeNull();
        actual.Value.ShouldBe(new(2025, 07, 27, 09, 44, 32, 466, DateTimeKind.Utc));
    }

    [Fact]
    public static void Converter_Throws_If_Unsupported_Kind()
    {
        // Arrange
        string json =
            """
            {
                "Value": true
            }
            """;

        // Act and Assert
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Model>(json, Options));
    }

    private sealed class Model
    {
        [JsonConverter(typeof(MixedDateTimeConverter))]
        public DateTime Value { get; set; }
    }
}
