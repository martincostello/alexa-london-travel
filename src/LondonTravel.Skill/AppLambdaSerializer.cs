// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using Amazon.Lambda.Serialization.SystemTextJson;

namespace MartinCostello.LondonTravel.Skill;

public sealed class AppLambdaSerializer() : SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>()
{
    protected override JsonSerializerOptions CreateDefaultJsonSerializationOptions()
        => new(AppJsonSerializerContext.Default.Options);

    protected override T InternalDeserialize<T>(byte[] utf8Json)
    {
        using var document = JsonDocument.Parse(utf8Json);

        Console.WriteLine(document.RootElement.ToString());

        return base.InternalDeserialize<T>(utf8Json);
    }

    protected override void InternalSerialize<T>(Utf8JsonWriter writer, T response)
    {
        Console.WriteLine(response.GetType().ToString());
        base.InternalSerialize(writer, response);
    }
}
