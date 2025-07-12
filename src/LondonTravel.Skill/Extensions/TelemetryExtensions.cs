// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MartinCostello.LondonTravel.Skill.Extensions;

internal static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        services.AddSingleton((_) => SkillTelemetry.ActivitySource);
        services.AddOpenTelemetry()
                .WithMetrics((builder) =>
                {
                    builder.SetResourceBuilder(SkillTelemetry.ResourceBuilder)
                           .AddHttpClientInstrumentation()
                           .AddProcessInstrumentation()
                           .AddMeter("System.Runtime");

                    if (AlexaFunction.IsRunningInAwsLambda())
                    {
                        builder.AddOtlpExporter();
                    }
                })
                .WithTracing((builder) =>
                {
                    builder.SetResourceBuilder(SkillTelemetry.ResourceBuilder)
                           .AddSource(SkillTelemetry.ServiceName)
                           .AddHttpClientInstrumentation();

                    if (AlexaFunction.IsRunningInAwsLambda())
                    {
                        builder.AddAWSLambdaConfigurations((p) => p.DisableAwsXRayContextExtraction = true)
                               .AddOtlpExporter();
                    }
                });

        services.AddOptions<HttpClientTraceInstrumentationOptions>()
                .Configure((options) =>
                {
                    options.EnrichWithHttpRequestMessage = EnrichHttpActivity;
                    options.RecordException = true;
                });

        return services;
    }

    private static void EnrichHttpActivity(Activity activity, HttpRequestMessage request)
    {
        if (GetTag("server.address", activity.Tags) is { Length: > 0 } hostName)
        {
            activity.AddTag("peer.service", hostName);
        }

        static string? GetTag(string name, IEnumerable<KeyValuePair<string, string?>> tags)
            => tags.FirstOrDefault((p) => p.Key == name).Value;
    }
}
