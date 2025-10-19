// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.AWSLambda;
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

        var builder = services.AddOpenTelemetry();

        if (isRunningInLambda)
        {
            builder.UseOtlpExporter();
        }

        builder.WithTracing((builder) =>
        {
            builder.SetResourceBuilder(SkillTelemetry.ResourceBuilder)
                   .AddSource(SkillTelemetry.ServiceName)
                   .AddAWSInstrumentation();

            builder.AddHttpClientInstrumentation((options) =>
            {
                options.FilterHttpRequestMessage = FilterHttpRequest;
                options.RecordException = true;
            });

            if (isRunningInLambda)
            {
                builder.AddAWSLambdaConfigurations((p) => p.DisableAwsXRayContextExtraction = true);
            }
        });

        return services;
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
