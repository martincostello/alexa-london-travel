// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MartinCostello.LondonTravel.Skill.Extensions;

internal static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        services.AddSingleton((_) => SkillTelemetry.ActivitySource);
        services.AddOpenTelemetry()
                .ConfigureResource((builder) => builder.AddService(SkillTelemetry.ServiceName, serviceVersion: SkillTelemetry.ServiceVersion))
                .WithTracing((builder) =>
                {
                    builder.AddHttpClientInstrumentation((p) => p.RecordException = true)
                           .AddSource(SkillTelemetry.ServiceName);

                    if (IsRunningInAwsLambda())
                    {
                        builder.AddAWSLambdaConfigurations()
                               .AddOtlpExporter();
                    }
                });

        return services;
    }

    private static bool IsRunningInAwsLambda()
        => Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME") is { Length: > 0 } &&
           Environment.GetEnvironmentVariable("AWS_REGION") is { Length: > 0 };
}
