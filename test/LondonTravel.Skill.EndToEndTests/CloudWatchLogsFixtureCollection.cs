// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace LondonTravel.Skill.EndToEndTests;

[CollectionDefinition(Name)]
public class CloudWatchLogsFixtureCollection : ICollectionFixture<CloudWatchLogsFixture>
{
    public const string Name = "CloudWatch Logs";
}
