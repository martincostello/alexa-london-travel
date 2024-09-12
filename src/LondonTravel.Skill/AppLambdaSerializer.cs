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
            Console.WriteLine(Convert.ToBase64String(utf8Json));
            return base.InternalDeserialize<T>(utf8Json);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    protected override void InternalSerialize<T>(Utf8JsonWriter writer, T response)
    {
        try
        {
            using (var stream = new MemoryStream())
            {
                using var writer2 = new Utf8JsonWriter(stream, WriterOptions);
                base.InternalSerialize(writer2, response);
                stream.Position = 0L;

                Console.WriteLine(Convert.ToBase64String(stream.ToArray()));
            }

            Console.WriteLine(response.GetType().ToString());
            base.InternalSerialize(writer, response);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}
