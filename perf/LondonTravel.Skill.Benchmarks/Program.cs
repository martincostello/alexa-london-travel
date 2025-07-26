// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;
using MartinCostello.LondonTravel.Skill.Benchmarks;

if (args.SequenceEqual(["--test"]))
{
    await using var benchmark = new AppBenchmarks();
    await benchmark.StartServer();

    try
    {
        _ = await benchmark.Cancel();
        _ = await benchmark.Commute();
        _ = await benchmark.Disruption();
        _ = await benchmark.Help();
        _ = await benchmark.Launch();
        _ = await benchmark.SessionEnded();
        _ = await benchmark.Status();
        _ = await benchmark.Stop();
        _ = await benchmark.UnknownIntent();
    }
    finally
    {
        await benchmark.StopServer();
    }
}
else
{
    BenchmarkRunner.Run<AppBenchmarks>(args: args);
}
