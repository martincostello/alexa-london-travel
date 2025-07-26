// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

#pragma warning disable IDE0130
namespace MartinCostello.LondonTravel.Skill;

internal static class SecretsManager
{
    private const string SecretsManagerContentType = "application/x-amz-json-1.1";

    private static readonly Dictionary<string, string> Secrets = new()
    {
        ["alexa-london-travel/OTEL_EXPORTER_OTLP_HEADERS"] = string.Empty,
        ["alexa-london-travel/Skill__SkillId"] = "alexa-london-travel",
        ["alexa-london-travel/Skill__TflApplicationId"] = "tfl-application-id",
        ["alexa-london-travel/Skill__TflApplicationKey"] = "tfl-application-key",
    };

    public static void AddEndpoints(IEndpointRouteBuilder builder)
        => builder.MapPost("/", HandleAsync)
                  .Accepts<SecretsManagerRequest>(SecretsManagerContentType);

    private static async Task HandleAsync(
        [FromHeader(Name = "X-Amz-Target")] string target,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        const string DescribeSecret = "secretsmanager.DescribeSecret";
        const string GetSecretValue = "secretsmanager.GetSecretValue";

        if (target is not (DescribeSecret or GetSecretValue))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var request = await JsonSerializer.DeserializeAsync<SecretsManagerRequest>(
            context.Request.Body,
            cancellationToken: cancellationToken);

        if (request?.SecretId is not { Length: > 0 } name ||
            !Secrets.TryGetValue(name, out string? secretString))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        const string SecretVersion = "1";

        object response = target switch
        {
            DescribeSecret => new DescribeSecret()
            {
                Name = name,
                VersionIdsToStages = new() { [SecretVersion] = ["AWSCURRENT"] },
            },
            GetSecretValue => new GetSecretValue()
            {
                Name = name,
                SecretString = secretString ?? string.Empty,
                VersionId = SecretVersion,
            },
            _ => throw new NotImplementedException(),
        };

        context.Response.GetTypedHeaders().ContentType = new(SecretsManagerContentType);

        await context.Response.WriteAsJsonAsync(response, cancellationToken);
    }

    private sealed class SecretsManagerRequest
    {
        [JsonPropertyName("SecretId")]
        public string? SecretId { get; set; }
    }

    /// <summary>
    /// See https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_DescribeSecret.html.
    /// </summary>
    private sealed class DescribeSecret
    {
        [JsonPropertyName("ARN")]
        public string Arn => $"arn:aws:secretsmanager:eu-west-1:01234567890:secret:{Name}";

        [JsonPropertyName("Name")]
        public required string Name { get; init; }

        [JsonPropertyName("VersionIdsToStages")]
        public Dictionary<string, string[]> VersionIdsToStages { get; init; } = [];
    }

    /// <summary>
    /// See https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html.
    /// </summary>
    private sealed class GetSecretValue
    {
        [JsonPropertyName("ARN")]
        public string Arn => $"arn:aws:secretsmanager:eu-west-1:01234567890:secret:{Name}";

        [JsonPropertyName("Name")]
        public required string Name { get; init; }

        [JsonPropertyName("SecretString")]
        public required string SecretString { get; init; }

        [JsonPropertyName("VersionId")]
        public required string VersionId { get; init; }
    }
}
