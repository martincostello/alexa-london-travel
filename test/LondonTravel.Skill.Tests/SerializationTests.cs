// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using Amazon.Lambda.Serialization.SystemTextJson;
using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill;

public static class SerializationTests
{
    [Theory]
    [InlineData("IntentRequest", typeof(IntentRequest))]
    [InlineData("LaunchRequest", typeof(LaunchRequest))]
    [InlineData("LaunchRequestWithEpochTimestamp", typeof(LaunchRequest))]
    [InlineData("SessionEndedRequest", typeof(SessionEndedRequest))]
    public static void Can_Deserialize_Request(string name, Type expectedType)
    {
        // Arrange
        JsonSerializer.IsReflectionEnabledByDefault.ShouldBeFalse();

        var serializer = new AppLambdaSerializer();
        using var stream = File.OpenRead(Path.Combine("Samples", $"{name}.json"));

        // Act
        var actual = serializer.Deserialize<SkillRequest>(stream);

        // Assert
        actual.ShouldNotBeNull();
        actual.Request.ShouldNotBeNull();
        actual.Request.ShouldBeOfType(expectedType);
    }

    [Fact]
    public static void Can_Deserialize_Request_From_Non_Seekable_Stream()
    {
        // Arrange
        var serializer = new AppLambdaSerializer();
        using var file = File.OpenRead(Path.Combine("Samples", "IntentRequest.json"));
        using var stream = new NonSeekableStream(file);

        // Act
        var actual = serializer.Deserialize<SkillRequest>(stream);

        // Assert
        actual.ShouldNotBeNull();
        actual.Request.ShouldBeOfType<IntentRequest>();
    }

    [Fact]
    public static void Deserialize_Throws_If_Json_Is_Invalid()
    {
        // Arrange
        var serializer = new AppLambdaSerializer();
        using var stream = new MemoryStream("{"u8.ToArray());

        // Act and Assert
        var exception = Should.Throw<JsonSerializerException>(() => serializer.Deserialize<SkillRequest>(stream));
        exception.InnerException.ShouldBeAssignableTo<JsonException>();
    }

    [Fact]
    public static void Deserialize_Throws_If_Type_Is_Not_Registered()
    {
        // Arrange
        var serializer = new AppLambdaSerializer();
        using var stream = new MemoryStream("{}"u8.ToArray());

        // Act and Assert
        Should.Throw<JsonSerializerException>(() => serializer.Deserialize<UnregisteredType>(stream));
    }

    [Fact]
    public static void Serialize_Throws_If_Type_Is_Not_Registered()
    {
        // Arrange
        var serializer = new AppLambdaSerializer();
        using var stream = new MemoryStream();

        // Act and Assert
        Should.Throw<JsonSerializerException>(() => serializer.Serialize(new UnregisteredType(), stream));
    }

    [Fact]
    public static void Can_Serialize_Multiple_Responses()
    {
        // Arrange
        var serializer = new AppLambdaSerializer();

        string[] expected = ["<p>One</p>", "<p>Two</p>"];

        foreach (string ssml in expected)
        {
            var response = new SkillResponse()
            {
                Response = new()
                {
                    OutputSpeech = new() { Ssml = ssml },
                },
            };

            using var stream = new MemoryStream();

            // Act
            serializer.Serialize(response, stream);

            // Assert
            using var document = JsonDocument.Parse(stream.ToArray());

            document.RootElement
                .GetProperty("response")
                .GetProperty("outputSpeech")
                .GetProperty("ssml")
                .GetString()
                .ShouldBe(ssml);
        }
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

    private sealed class UnregisteredType;

    private sealed class NonSeekableStream(Stream inner) : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
