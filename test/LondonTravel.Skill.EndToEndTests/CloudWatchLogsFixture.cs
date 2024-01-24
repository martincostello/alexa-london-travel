// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Http.Json;
using Amazon;
using Amazon.CloudWatchLogs;
using Xunit.Sdk;

namespace LondonTravel.Skill.EndToEndTests;

public class CloudWatchLogsFixture(IMessageSink diagnosticMessageSink) : IAsyncLifetime
{
    private readonly DateTime _started = TimeProvider.System.GetUtcNow().UtcDateTime;

    internal Dictionary<string, string> Requests { get; } = [];

    public async Task DisposeAsync()
    {
        if (Requests.Count < 1)
        {
            return;
        }

        var credentials = TestConfiguration.GetCredentials();
        string? functionName = TestConfiguration.FunctionName;
        string? regionName = TestConfiguration.RegionName;

        if (functionName is not null &&
            regionName is not null &&
            credentials is not null)
        {
            var builder = new StringBuilder()
                .AppendLine()
                .AppendLine()
                .AppendLine(CultureInfo.InvariantCulture, $"AWS Request ID(s) (Count = {Requests.Count}):")
                .AppendLine();

            foreach ((string requestId, _) in Requests)
            {
                builder.AppendLine(CultureInfo.InvariantCulture, $"  - {requestId}");
            }

            diagnosticMessageSink.OnMessage(new DiagnosticMessage(builder.ToString()));

            var delay = TimeSpan.FromSeconds(10);

            diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Waiting {delay.Seconds} seconds for CloudWatch logs..."));
            await Task.Delay(delay);

            string logGroupName = $"/aws/lambda/{functionName}";
            var region = RegionEndpoint.GetBySystemName(regionName);

            using var logsClient = new AmazonCloudWatchLogsClient(credentials, region);

            var groups = await logsClient.DescribeLogStreamsAsync(new()
            {
                Descending = true,
                Limit = Math.Max(5, Requests.Count),
                LogGroupName = logGroupName,
                OrderBy = OrderBy.LastEventTime,
            });

            var logs = new List<(LogEvent Event, string Message)>();

            foreach (var stream in groups.LogStreams)
            {
                if (logs.Count >= Requests.Count)
                {
                    break;
                }

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
                    string requestIdLine = split[0][ReportPrefix.Length..];
                    string requestId = requestIdLine.Split(' ')[1];

                    if (!Requests.TryGetValue(requestId, out string? name))
                    {
                        continue;
                    }

                    var entry = new LogEvent(name)
                    {
                        RequestId = requestId,
                        Timestamp = @event.Timestamp,
                    };

                    builder
                        .Clear()
                        .AppendLine()
                        .AppendLine()
                        .AppendFormat(CultureInfo.InvariantCulture, $"Timestamp: {@event.Timestamp:u}")
                        .AppendLine()
                        .AppendLine(requestIdLine);

                    foreach (string value in split.Skip(1))
                    {
                        if (value.Trim() is { Length: > 0 } trimmed)
                        {
                            builder.AppendLine(trimmed);

                            string[] parts = trimmed.Split(": ");

                            switch (parts[0])
                            {
                                case "Duration":
                                    entry.Duration = parts[1];
                                    break;

                                case "Billed Duration":
                                    entry.BilledDuration = parts[1];
                                    break;

                                case "Memory Size":
                                    entry.MemorySize = parts[1];
                                    break;

                                case "Max Memory Used":
                                    entry.MaxMemoryUsed = parts[1];
                                    break;

                                case "Init Duration":
                                    entry.InitDuration = parts[1];
                                    break;
                            }
                        }
                    }

                    logs.Add(new(entry, builder.ToString()));
                }
            }

            var requestIds = Requests.Keys.ToList();
            var events = logs
                .Where((p) => requestIds.Contains(p.Event.RequestId))
                .OrderBy((p) => p.Event.Timestamp)
                .ToList();

            diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Found {events.Count} CloudWatch log events."));

            foreach ((_, string message) in events)
            {
                diagnosticMessageSink.OnMessage(new DiagnosticMessage(message));
            }

            await TryPostLogsToPullRequestAsync(events.Select((p) => p.Event));
        }
    }

    public Task InitializeAsync() => Task.CompletedTask;

    private static async Task TryPostLogsToPullRequestAsync(IEnumerable<LogEvent> events)
    {
        string? apiUrl = Environment.GetEnvironmentVariable("GITHUB_API_URL");
        string? repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
        string? token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        string? issue = Environment.GetEnvironmentVariable("PULL_NUMBER");

        if (string.IsNullOrEmpty(apiUrl) ||
            string.IsNullOrEmpty(repository) ||
            string.IsNullOrEmpty(issue) ||
            string.IsNullOrEmpty(token) ||
            !Uri.TryCreate(apiUrl, UriKind.Absolute, out var baseAddress))
        {
            return;
        }

        var comment = new StringBuilder()
            .AppendLine("| **Payload** | **Duration** | **Billed Duration** | **Memory Size** | **Max Memory Used** | **Init Duration** |")
            .AppendLine("|:------------|-------------:|--------------------:|----------------:|--------------------:|------------------:|");

        foreach (var entry in events)
        {
            comment
                .Append(CultureInfo.InvariantCulture, $"| `{entry.Name}` <!-- {entry.RequestId:u} {entry.Timestamp:u} -->")
                .Append(CultureInfo.InvariantCulture, $" | {entry.Duration}")
                .Append(CultureInfo.InvariantCulture, $" | {entry.BilledDuration}")
                .Append(CultureInfo.InvariantCulture, $" | {entry.MemorySize}")
                .Append(CultureInfo.InvariantCulture, $" | {entry.MaxMemoryUsed}")
                .Append(CultureInfo.InvariantCulture, $" | {entry.InitDuration ?? "-"}")
                .AppendLine(" |");
        }

        using var httpClient = new HttpClient()
        {
            BaseAddress = baseAddress,
            DefaultRequestHeaders =
            {
                Accept = { new("application/vnd.github+json") },
                Authorization = new("Bearer", token),
                UserAgent = { TestConfiguration.UserAgent },
            },
        };

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-GitHub-Api-Version", "2022-11-28");

        using var response = await httpClient.PostAsJsonAsync(
            $"repos/{repository}/issues/{Uri.EscapeDataString(issue)}/comments",
            new { body = comment.ToString() });

        response.EnsureSuccessStatusCode();
    }

    private sealed class LogEvent(string name)
    {
        public string Name => name;

        public DateTime Timestamp { get; set; }

        public string RequestId { get; set; } = default!;

        public string Duration { get; set; } = default!;

        public string BilledDuration { get; set; } = default!;

        public string MemorySize { get; set; } = default!;

        public string MaxMemoryUsed { get; set; } = default!;

        public string InitDuration { get; set; } = default!;
    }
}
