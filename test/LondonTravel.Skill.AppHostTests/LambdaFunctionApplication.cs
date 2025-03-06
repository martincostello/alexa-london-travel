// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MartinCostello.LondonTravel.Skill.AppHostTests;

internal sealed class LambdaFunctionApplication(string serverUrl, Action<IServiceCollection> configureServices)
    : DistributedApplicationFactory(typeof(Projects.LondonTravel_Skill))
{
    public Dictionary<string, string> Configuration { get; } = new()
    {
        ["AWS_ACCESS_KEY_ID"] = "aws-access-key-id",
        ["AWS_SECRET_ACCESS_KEY"] = "aws-secret-access-key",
        ["AWS_ENDPOINT_URL_SECRETS_MANAGER"] = serverUrl,
        ["Skill:SkillApiUrl"] = serverUrl,
        ["Skill:TflApiUrl"] = serverUrl,
    };

    protected override void OnBuilderCreating(
        DistributedApplicationOptions applicationOptions,
        HostApplicationBuilderSettings hostOptions)
    {
        applicationOptions.Args = [.. Configuration.Select((p) => $"--{p.Key}={p.Value}")];
    }

    protected override void OnBuilding(DistributedApplicationBuilder applicationBuilder)
        => configureServices(applicationBuilder.Services);
}
