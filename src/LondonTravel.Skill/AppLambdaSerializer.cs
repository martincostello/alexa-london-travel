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
        try
        {
            Console.WriteLine($"Request: {Convert.ToBase64String(utf8Json)}");
            var result = base.InternalDeserialize<T>(utf8Json);

            Console.WriteLine($"Request: {result.GetType()}");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    protected override void InternalSerialize<T>(Utf8JsonWriter writer, T response)
    {
        try
        {
            Console.WriteLine($"Response: {response.GetType()}");

            using (var stream = new MemoryStream())
            {
                using var writer2 = new Utf8JsonWriter(stream, WriterOptions);
                base.InternalSerialize(writer2, response);
                stream.Position = 0L;
                Console.WriteLine($"Response: {Convert.ToBase64String(stream.ToArray())}");
            }

            base.InternalSerialize(writer, response);

            Console.WriteLine("Response serialized");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}
