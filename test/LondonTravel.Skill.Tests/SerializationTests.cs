// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;

namespace MartinCostello.LondonTravel.Skill;

public static class SerializationTests
{
    [Theory]
    [InlineData("IntentRequest")]
    [InlineData("LaunchRequest")]
    [InlineData("LaunchRequestWithEpochTimestamp")]
    [InlineData("SessionEndedRequest")]
    public static async Task Can_Deserialize_Request(string name)
    {
        // Arrange
        string json = await File.ReadAllTextAsync(Path.Combine("Samples", $"{name}.json"));

        // Act
        var actual = JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.SkillRequest);

        // Assert
        actual.ShouldNotBeNull();
        actual.Request.ShouldNotBeNull();
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
