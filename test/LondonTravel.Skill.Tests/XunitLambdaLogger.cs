// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.Lambda.Core;

namespace MartinCostello.LondonTravel.Skill;

internal class XunitLambdaLogger : ILambdaLogger
{
    internal XunitLambdaLogger(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
    }

    private ITestOutputHelper OutputHelper { get; }

    public void Log(string message)
    {
        OutputHelper.WriteLine(message);
    }

    public void LogLine(string message)
    {
        OutputHelper.WriteLine(message);
    }
}
