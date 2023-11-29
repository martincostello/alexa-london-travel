// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using MartinCostello.LondonTravel.Skill.Models;

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

    [Fact]
    public static void Can_Serialize_Response_With_No_Card()
    {
        // Arrange
        var response = new SkillResponse()
        {
            Response = new()
            {
                OutputSpeech = new()
                {
                    Ssml = "<p>Hello, world!</p>",
                },
            },
        };

        // Act
        string actual = JsonSerializer.Serialize(response, AppJsonSerializerContext.Default.SkillResponse);

        // Assert
        actual.ShouldNotBeNull();
        using var document = JsonDocument.Parse(actual);

        document.RootElement
            .GetProperty("response")
            .TryGetProperty("card", out _)
            .ShouldBeFalse();
    }

    [Fact]
    public static void Can_Serialize_Response_With_Link_Account_Card()
    {
        // Arrange
        var response = new SkillResponse()
        {
            Response = new()
            {
                Card = new LinkAccountCard(),
                OutputSpeech = new()
                {
                    Ssml = "<p>Hello, world!</p>",
                },
            },
        };

        // Act
        string actual = JsonSerializer.Serialize(response, AppJsonSerializerContext.Default.SkillResponse);

        // Assert
        actual.ShouldNotBeNull();
        using var document = JsonDocument.Parse(actual);

        var card = document.RootElement
            .GetProperty("response")
            .GetProperty("card");

        card.GetProperty("type").GetString().ShouldBe("LinkAccount");
        card.EnumerateObject().Count().ShouldBe(1);
    }

    [Fact]
    public static void Can_Serialize_Response_With_Standard_Card()
    {
        // Arrange
        var response = new SkillResponse()
        {
            Response = new()
            {
                Card = new StandardCard()
                {
                    Content = "Hello, world!",
                    Title = "Hello",
                },
                OutputSpeech = new()
                {
                    Ssml = "<p>Hello, world!</p>",
                },
            },
        };

        // Act
        string actual = JsonSerializer.Serialize(response, AppJsonSerializerContext.Default.SkillResponse);

        // Assert
        actual.ShouldNotBeNull();
        using var document = JsonDocument.Parse(actual);

        var card = document.RootElement
            .GetProperty("response")
            .GetProperty("card");

        card.GetProperty("type").GetString().ShouldBe("Standard");
        card.GetProperty("title").GetString().ShouldBe("Hello");
        card.GetProperty("text").GetString().ShouldBe("Hello, world!");
        card.EnumerateObject().Count().ShouldBe(3);
    }
}
