// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;

namespace MartinCostello.LondonTravel.Skill;

public sealed class SkillMetrics : IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<long> _skillInvocationCounter;

    public SkillMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(SkillTelemetry.ServiceName, SkillTelemetry.ServiceVersion);

        _skillInvocationCounter = _meter.CreateCounter<long>(
            "londontravel.skill.invocations",
            unit: "{count}",
            description: "The number of Alexa Skill invocations.");
    }

    public void Dispose() => _meter?.Dispose();

    public void SkillInvoked(string? requestType)
        => _skillInvocationCounter.Add(1, new KeyValuePair<string, object?>("alexa.request.type", requestType));
}
