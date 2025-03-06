// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MartinCostello.LondonTravel.Skill.AppHostTests;

internal static class TflApi
{
    private const string ApplicationId = "tfl-application-id";
    private const string ApplicationKey = "tfl-application-key";

    public static void AddEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/Line/Mode/{modes}/Disruption", GetDisruptionAsync);
        builder.MapGet("/Line/{line}/Status", GetStatusAsync);
    }

    private static async Task GetDisruptionAsync(
        [FromRoute] string modes,
        [FromQuery(Name = "app_id")] string? applicationId,
        [FromQuery(Name = "app_key")] string? applicationKey,
        HttpResponse response)
    {
        if (applicationId is not ApplicationId || applicationKey is not ApplicationKey)
        {
            response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else if (modes?.Split(',').Length < 1)
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
        [FromQuery(Name = "app_id")] string? applicationId,
        [FromQuery(Name = "app_key")] string? applicationKey,
        HttpResponse response)
    {
        if (applicationId is not ApplicationId || applicationKey is not ApplicationKey)
        {
            response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else if (string.IsNullOrEmpty(line))
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
