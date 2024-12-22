// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;

namespace MartinCostello.LondonTravel.Skill;

public static class InteractionModelTests
{
    [Fact]
    public static async Task Interaction_Model_Is_Valid_Json()
    {
        // Arrange
        var type = typeof(InteractionModelTests);
        var assembly = type.Assembly;

        using var model = assembly.GetManifestResourceStream(type.Namespace + ".interaction-model.json")!;
        using var stream = new MemoryStream();

        await model.CopyToAsync(stream, TestContext.Current.CancellationToken);
        model.Seek(0, SeekOrigin.Begin);

        var reader = new Utf8JsonReader(stream.ToArray());

        // Act
        bool actual = JsonDocument.TryParseValue(ref reader, out _);

        // Assert
        actual.ShouldBeTrue();
    }
}
