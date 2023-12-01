// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon;
using Amazon.CloudWatchLogs;
using Xunit.Sdk;

namespace LondonTravel.Skill.EndToEndTests;

public class CloudWatchLogsFixture(IMessageSink diagnosticMessageSink) : IAsyncLifetime
{
    private readonly DateTime _started = TimeProvider.System.GetUtcNow().UtcDateTime;

    public IList<string> RequestIds { get; } = new List<string>();

    public async Task DisposeAsync()
    {
        var credentials = AwsConfiguration.GetCredentials();
        string functionName = AwsConfiguration.FunctionName;
        string regionName = AwsConfiguration.RegionName;

        if (functionName is not null &&
            regionName is not null &&
            credentials is not null)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));

            string logGroupName = $"/aws/lambda/{functionName}";
            var region = RegionEndpoint.GetBySystemName(regionName);

            using var logsClient = new AmazonCloudWatchLogsClient(credentials, region);

            var groups = await logsClient.DescribeLogStreamsAsync(new()
            {
                Descending = true,
                Limit = Math.Max(5, RequestIds.Count),
                LogGroupName = logGroupName,
                OrderBy = OrderBy.LastEventTime,
            });

            var logs = new List<(DateTime Timestamp, string RequestId, string Message)>();

            foreach (var stream in groups.LogStreams)
            {
                var logEvents = await logsClient.GetLogEventsAsync(new()
                {
                    LogGroupName = logGroupName,
                    LogStreamName = stream.LogStreamName,
                });

                const string ReportPrefix = "REPORT ";

                var reports = logEvents.Events
                    .Where((p) => p.Timestamp >= _started)
                    .Where((p) => p.Message.StartsWith(ReportPrefix, StringComparison.Ordinal))
                    .ToList();

                foreach (var @event in reports)
                {
                    string[] split = @event.Message.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                    string requestId = split[0][ReportPrefix.Length..];

                    var builder = new StringBuilder()
                        .AppendLine()
                        .AppendLine()
                        .AppendFormat(CultureInfo.InvariantCulture, $"Timestamp: {@event.Timestamp:u}")
                        .AppendLine()
                        .AppendLine(requestId);

                    foreach (string value in split.Skip(1))
                    {
                        if (value.Trim() is { Length: > 0 } trimmed)
                        {
                            builder.AppendLine(trimmed);
                        }
                    }

                    logs.Add(new(@event.Timestamp, requestId, builder.ToString()));
                }
            }

            var events = logs
                .Where((p) => RequestIds.Contains(p.RequestId))
                .OrderBy((p) => p.Timestamp)
                .ToList();

            foreach (var (_, _, message) in events)
            {
                diagnosticMessageSink.OnMessage(new DiagnosticMessage(message));
            }
        }
    }

    public Task InitializeAsync() => Task.CompletedTask;
}
