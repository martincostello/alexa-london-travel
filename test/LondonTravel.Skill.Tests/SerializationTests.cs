// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill;

public static class SerializationTests
{
    [Theory]
    [InlineData("IntentRequest", typeof(IntentRequest))]
    [InlineData("LaunchRequest", typeof(LaunchRequest))]
    [InlineData("LaunchRequestWithEpochTimestamp", typeof(LaunchRequest))]
    [InlineData("SessionEndedRequest", typeof(SessionEndedRequest))]
    public static async Task Can_Deserialize_Request(string name, Type expectedType)
    {
        // Arrange
        string json = await File.ReadAllTextAsync(Path.Combine("Samples", $"{name}.json"));

        // Act
        var actual = JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.SkillRequest);

        // Assert
        actual.ShouldNotBeNull();
        actual.Request.ShouldNotBeNull();
        actual.Request.ShouldBeOfType(expectedType);
    }

    [Theory]
    [InlineData("SkillResponse")]
    public static async Task Can_Deserialize_Response(string name)
    {
        // Arrange
        string json = await File.ReadAllTextAsync(Path.Combine("Samples", $"{name}.json"));

        // Act
        var actual = JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.ResponseBody);

        // Assert
        actual.ShouldNotBeNull();
    }
}
