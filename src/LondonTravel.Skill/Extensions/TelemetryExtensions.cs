// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MartinCostello.LondonTravel.Skill.Extensions;

internal static class TelemetryExtensions
{
    private static readonly Uri? RuntimeApiBaseAddress = GetRuntimeApiUri();

    public static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        services.AddSingleton((_) => SkillTelemetry.ActivitySource);

        bool isRunningInLambda = AlexaFunction.IsRunningInAwsLambda();
        bool metricsEnabled = SkillTelemetry.MetricsEnabled;

        var builder = services.AddOpenTelemetry();

        if (isRunningInLambda)
        {
            builder.UseOtlpExporter();
        }

        if (metricsEnabled)
        {
            services.AddMetrics();
            services.AddSingleton<SkillMetrics>();

            builder.WithMetrics((builder) =>
            {
                builder.SetResourceBuilder(SkillTelemetry.ResourceBuilder)
                       .AddAWSInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddProcessInstrumentation()
                       .AddMeter(SkillTelemetry.ServiceName)
                       .AddMeter("System.Runtime");
            });
        }

        builder.WithTracing((builder) =>
               {
                   builder.SetResourceBuilder(SkillTelemetry.ResourceBuilder)
                          .AddSource(SkillTelemetry.ServiceName)
                          .AddAWSInstrumentation()
                          .AddHttpClientInstrumentation();

                   if (isRunningInLambda)
                   {
                       builder.AddAWSLambdaConfigurations((p) => p.DisableAwsXRayContextExtraction = true);
                   }
               });

        services.AddOptions<HttpClientTraceInstrumentationOptions>()
                .Configure((options) =>
                {
                    options.EnrichWithHttpRequestMessage = EnrichHttpActivity;
                    options.FilterHttpRequestMessage = FilterHttpRequest;
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

    private static bool FilterHttpRequest(HttpRequestMessage message)
    {
        if (RuntimeApiBaseAddress is { } baseAddress &&
            message.RequestUri is { } uri)
        {
            return !baseAddress.IsBaseOf(uri);
        }

        return true;
    }

    private static Uri? GetRuntimeApiUri()
    {
        string? runtimeApiUrl = Environment.GetEnvironmentVariable("AWS_LAMBDA_RUNTIME_API");
        Uri? baseAddress = null;

        if (!string.IsNullOrWhiteSpace(runtimeApiUrl))
        {
            baseAddress = new UriBuilder(runtimeApiUrl).Uri;
        }

        return baseAddress;
    }
}
