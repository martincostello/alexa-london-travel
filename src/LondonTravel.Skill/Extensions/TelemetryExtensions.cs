// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Trace;

namespace MartinCostello.LondonTravel.Skill.Extensions;

internal static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        services.AddSingleton((_) => SkillTelemetry.ActivitySource);
        services.AddOpenTelemetry()
                .WithTracing((builder) =>
                {
                    builder.SetResourceBuilder(SkillTelemetry.ResourceBuilder)
                           .AddSource(SkillTelemetry.ServiceName)
                           .AddHttpClientInstrumentation((p) => p.RecordException = true);

                    if (AlexaFunction.IsRunningInAwsLambda())
                    {
                        builder.AddAWSLambdaConfigurations()
                               .AddOtlpExporter();
                    }
                });

        return services;
    }
}
