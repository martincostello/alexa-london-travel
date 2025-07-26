// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

#pragma warning disable IDE0130
namespace MartinCostello.LondonTravel.Skill;

internal static class TflApi
{
    public static void AddEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("Line")
            .AddEndpointFilter(async (context, next) =>
            {
                if (context.HttpContext.Request.Query.TryGetValue("app_id", out var appId) &&
                    context.HttpContext.Request.Query.TryGetValue("app_key", out var appKey) &&
                    appId == "tfl-application-id" &&
                    appKey == "tfl-application-key")
                {
                    return await next(context);
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return ValueTask.CompletedTask;
            });

        group.MapGet("Mode/{modes}/Disruption", GetDisruptionAsync);
        group.MapGet("{line}/Status", GetStatusAsync);
    }

    private static async Task GetDisruptionAsync(
        [FromRoute] string modes,
        HttpResponse response)
    {
        if (modes?.Split(',').Length < 1)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
        }
        else
        {
            var result = new[]
            {
                new { description = "There are severe delays on the District Line." },
            };

            await response.WriteAsJsonAsync(result);
        }
    }

    private static async Task GetStatusAsync(
        [FromRoute] string line,
        HttpResponse response)
    {
        if (string.IsNullOrEmpty(line))
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
        }
        else if (line is not ("district" or "northern"))
        {
            response.StatusCode = StatusCodes.Status404NotFound;
        }
        else
        {
            var result = new[]
            {
                new
                {
                    id = line,
                    name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(line),
                    modeName = "tube",
                    disruptions = Array.Empty<string>(),
                    lineStatuses = new[]
                    {
                        new
                        {
                          id = 0,
                          statusSeverity = 10,
                          statusSeverityDescription = "Good Service",
                          created = DateTimeOffset.UtcNow,
                          validityPeriods = Array.Empty<string>(),
                        },
                    },
                },
            };

            await response.WriteAsJsonAsync(result);
        }
    }
}
