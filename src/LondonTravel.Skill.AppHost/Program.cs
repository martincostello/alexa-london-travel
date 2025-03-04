// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

#pragma warning disable CA2252

using Amazon;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var aws = builder.AddAWSSDKConfig()
                 .WithRegion(RegionEndpoint.EUWest1);

var function = builder.AddAWSLambdaFunction<Projects.LondonTravel_Skill>("LondonTravelSkill", "LondonTravel.Skill")
                      .WithReference(aws);

string[] keys =
[
    "Skill:SkillApiUrl",
    "Skill:TflApplicationId",
    "Skill:TflApplicationKey",
];

foreach (string key in keys)
{
    string name = key.Replace(ConfigurationPath.KeyDelimiter, "__", StringComparison.Ordinal);
    function.WithEnvironment(name, builder.Configuration[key]);
}

var app = builder.Build();

app.Run();
