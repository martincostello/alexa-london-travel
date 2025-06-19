// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Trace;

namespace MartinCostello.LondonTravel.Skill.Extensions;

internal static class TelemetryExtensions
{
    private static readonly ConcurrentDictionary<string, string> ServiceMap = new(StringComparer.OrdinalIgnoreCase);

    public static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        services.AddSingleton((_) => SkillTelemetry.ActivitySource);
        services.AddOpenTelemetry()
                .WithTracing((builder) =>
                {
                    builder.SetResourceBuilder(SkillTelemetry.ResourceBuilder)
                           .AddSource(SkillTelemetry.ServiceName)
                           .AddHttpClientInstrumentation();

                    if (AlexaFunction.IsRunningInAwsLambda())
                    {
                        builder.AddAWSLambdaConfigurations()
                               .AddOtlpExporter();
                    }
                });

        services.AddOptions<HttpClientTraceInstrumentationOptions>()
                .Configure<IServiceProvider>((options, provider) =>
                {
                    AddServiceMappings(ServiceMap, provider);

                    options.EnrichWithHttpRequestMessage = EnrichHttpActivity;
                    options.RecordException = true;
                });

        return services;
    }

    private static void EnrichHttpActivity(Activity activity, HttpRequestMessage request)
    {
        TryEnrichWithPeerService(activity);

        static void TryEnrichWithPeerService(Activity activity)
        {
            if (GetTag("server.address", activity.Tags) is { Length: > 0 } hostName)
            {
                if (!ServiceMap.TryGetValue(hostName, out string? service))
                {
                    service = hostName;
                }

                activity.AddTag("peer.service", service);
            }
        }

        static string? GetTag(string name, IEnumerable<KeyValuePair<string, string?>> tags)
            => tags.FirstOrDefault((p) => p.Key == name).Value;
    }

    private static void AddServiceMappings(ConcurrentDictionary<string, string> mappings, IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var options = serviceProvider.GetRequiredService<IOptions<SkillConfiguration>>().Value;

        mappings[options.SkillApiUrl.Host] = "Skill API";
        mappings[options.TflApiUrl.Host] = "TfL API";

        if (configuration["AWS_LAMBDA_RUNTIME_API"] is { Length: > 0 } url &&
            Uri.TryCreate($"http://{url}", UriKind.Absolute, out var uri))
        {
            mappings[uri.Host] = "AWS Lambda Runtime API";
        }
    }
}
