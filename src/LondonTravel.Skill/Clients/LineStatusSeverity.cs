// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Skill.Clients
{
    /// <summary>
    /// An enumeration of line status severities.
    /// </summary>
    internal enum LineStatusSeverity
    {
        SpecialService = 0,

        Closed = 1,

        Suspended = 2,

        PartSuspended = 3,

        PlannedClosure = 4,

        PartClosure = 5,

        SevereDelays = 6,

        ReducedService = 7,

        BusService = 8,

        MinorDelays = 9,

        GoodService = 10,

        PartClosed = 11,

        ExitOnly = 12,

        NoStepFreeAccess = 13,

        ChangeOfFrequency = 14,

        Diverted = 15,

        NotRunning = 16,

        IssuesReported = 17,

        NoIssues = 18,

        Information = 19,

        ServiceClosed = 20,
    }
}
