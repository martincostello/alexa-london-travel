// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

#pragma warning disable IDE0130
namespace MartinCostello.LondonTravel.Skill;

internal static class UserPreferences
{
    public static void AddEndpoints(IEndpointRouteBuilder builder)
        => builder.MapGet("/api/preferences", GetPreferencesAsync);

    private static async Task GetPreferencesAsync(HttpContext context)
    {
        string? authorization = context.Request.Headers.Authorization;

        object response;

        if (authorization != "Bearer amzn1.ask.account.LAMBDA-TEST-DEV.token")
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            response = new ErrorResponse()
            {
                Message = "Unauthorized.",
                RequestId = context.TraceIdentifier,
                StatusCode = context.Response.StatusCode,
            };
        }
        else
        {
            response = new PreferencesResponse()
            {
                FavoriteLines = ["district"],
                UserId = "function-user-id",
            };
        }

        await context.Response.WriteAsJsonAsync(response);
    }

    private sealed class ErrorResponse
    {
        [JsonPropertyName("statusCode")]
        public required int StatusCode { get; init; }

        [JsonPropertyName("message")]
        public required string Message { get; init; }

        [JsonPropertyName("requestId")]
        public required string RequestId { get; init; }

        [JsonPropertyName("details")]
        public List<string> Details { get; set; } = [];
    }

    private sealed class PreferencesResponse
    {
        [JsonPropertyName("favoriteLines")]
        public required List<string> FavoriteLines { get; init; }

        [JsonPropertyName("userId")]
        public required string UserId { get; init; }
    }
}
