// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Buffers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class representing the <see cref="ILambdaSerializer"/> to use for the application. This class cannot be inherited.
/// </summary>
public sealed class AppLambdaSerializer : ILambdaSerializer
{
    /// <summary>
    /// The options to use for writing JSON, which match those used by <see cref="SourceGeneratorLambdaJsonSerializer{TSGContext}"/>. This field is read-only.
    /// </summary>
    private static readonly JsonWriterOptions WriterOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary>
    /// A cached <see cref="Utf8JsonWriter"/> for the current thread so that its buffer is reused between invocations.
    /// </summary>
    [ThreadStatic]
    private static Utf8JsonWriter? _writer;

    /// <inheritdoc/>
    public T Deserialize<T>(Stream requestStream)
    {
        try
        {
            var typeInfo = GetTypeInfo<T>();

            if (!requestStream.CanSeek)
            {
                return JsonSerializer.Deserialize(requestStream, typeInfo)!;
            }

            // Read the payload into a pooled buffer rather than copying it into a new array
            int length = checked((int)(requestStream.Length - requestStream.Position));
            byte[] buffer = ArrayPool<byte>.Shared.Rent(length);

            try
            {
                requestStream.ReadExactly(buffer, 0, length);
                return JsonSerializer.Deserialize(buffer.AsSpan(0, length), typeInfo)!;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        catch (Exception ex)
        {
            throw new JsonSerializerException($"Error converting the Lambda event JSON payload to type {typeof(T).FullName}: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public void Serialize<T>(T response, Stream responseStream)
    {
        var writer = GetWriter(responseStream);

        try
        {
            JsonSerializer.Serialize(writer, response, GetTypeInfo<T>());
        }
        catch (Exception ex)
        {
            throw new JsonSerializerException($"Error converting the response object of type {typeof(T).FullName} from the Lambda function to JSON: {ex.Message}", ex);
        }
        finally
        {
            // Do not keep the response stream alive until the next invocation
            writer.Reset(Stream.Null);
        }
    }

    private static Utf8JsonWriter GetWriter(Stream stream)
    {
        var writer = _writer;

        if (writer is null)
        {
            _writer = writer = new(stream, WriterOptions);
        }
        else
        {
            writer.Reset(stream);
        }

        return writer;
    }

    private static JsonTypeInfo<T> GetTypeInfo<T>()
        => AppJsonSerializerContext.Default.GetTypeInfo(typeof(T)) as JsonTypeInfo<T> ??
           throw new JsonSerializerException($"No JsonTypeInfo registered in {nameof(AppJsonSerializerContext)} for type {typeof(T).FullName}.");
}
