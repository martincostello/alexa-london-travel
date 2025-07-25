// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Http.Json;
using System.Text.Json;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Xunit.Sdk;
using Xunit.v3;

namespace MartinCostello.LondonTravel.Skill.EndToEndTests;

public class CloudWatchLogsFixture(IMessageSink diagnosticMessageSink) : IAsyncLifetime
{
    private readonly DateTime _started = TimeProvider.System.GetUtcNow().UtcDateTime;

    internal Dictionary<string, string> Requests { get; } = [];

    public async ValueTask DisposeAsync()
    {
        if (Requests.Count < 1)
        {
            return;
        }

        try
        {
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

                    var reports = logEvents.Events
                        .Where((p) => p.Timestamp >= _started)
                        .ToList();

                    foreach (var @event in reports)
                    {
                        if (TryParseEvent(@event, out var entry))
                        {
                            logs.Add(entry);
                        }
                    }
                }

                var requestIds = Requests.Keys.ToList();
                var events = logs
                    .Where((p) => requestIds.Contains(p.Event.RequestId))
                    .OrderBy((p) => p.Event.Timestamp)
                    .ToList();

                diagnosticMessageSink.OnMessage(new DiagnosticMessage() { Message = $"Found {events.Count} CloudWatch log events." });

                foreach ((_, string message) in events)
                {
                    diagnosticMessageSink.OnMessage(new DiagnosticMessage() { Message = message });
                }

                await TryPostLogsToPullRequestAsync(events.Select((p) => p.Event));
            }
        }
        catch (Exception ex)
        {
            diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Error retrieving CloudWatch logs: {ex}"));
        }

        GC.SuppressFinalize(this);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    private static async Task TryPostLogsToPullRequestAsync(IEnumerable<LogEvent> events)
    {
        string? apiUrl = Environment.GetEnvironmentVariable("GITHUB_API_URL");
        string? awsRegion = Environment.GetEnvironmentVariable("AWS_REGION");
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
            .AppendLine("| **Payload** | **Duration** | **Billed Duration** | **Memory Size** | **Max Memory Used** | **Init Duration** | **Trace** |")
            .AppendLine("|:------------|-------------:|--------------------:|----------------:|--------------------:|------------------:|:----------|");

        foreach (var entry in events)
        {
            comment
                .Append(CultureInfo.InvariantCulture, $"| `{entry.Name}` <!-- {entry.RequestId:u} {entry.Timestamp:u} -->")
                .Append(CultureInfo.InvariantCulture, $" | {entry.Duration}")
                .Append(CultureInfo.InvariantCulture, $" | {entry.BilledDuration}")
                .Append(CultureInfo.InvariantCulture, $" | {entry.MemorySize}")
                .Append(CultureInfo.InvariantCulture, $" | {entry.MaxMemoryUsed}")
                .Append(CultureInfo.InvariantCulture, $" | {entry.InitDuration ?? "-"}")
                .Append(CultureInfo.InvariantCulture, $" | {TraceUrl(entry.TraceId, awsRegion)}")
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

        static string TraceUrl(string? traceId, string? region)
        {
            if (string.IsNullOrEmpty(traceId))
            {
                return "-";
            }

            return $"[:link:](https://{region}.console.aws.amazon.com/cloudwatch/home?region={region}#xray:traces/{traceId})";
        }
    }

    private bool TryParseEvent(
        OutputLogEvent @event,
        out (LogEvent Event, string Message) entry)
    {
        return
            @event.Message.Length > 0 && @event.Message[0] == '{' ?
            TryParseEventAsJson(@event, out entry) :
            TryParseEventAsText(@event, out entry);
    }

    private bool TryParseEventAsJson(
        OutputLogEvent log,
        out (LogEvent Event, string Message) parsed)
    {
        parsed = default;

        PlatformEvent? @event;

        try
        {
            @event = JsonSerializer.Deserialize(log.Message, PlatformJsonSerializationContext.Default.PlatformEvent);
        }
        catch (NotSupportedException)
        {
            // Missing the type discriminator, so not a platform event
            return false;
        }

        if (@event is not PlatformReportEvent report ||
            report.Record.Metrics is not { } metrics ||
            report.Record.RequestId is not { Length: > 0 } requestId)
        {
            return false;
        }

        if (!Requests.TryGetValue(requestId, out string? name))
        {
            return false;
        }

        var entry = new LogEvent(name)
        {
            BilledDuration = metrics.BilledDurationMs.ToString(CultureInfo.InvariantCulture),
            Duration = metrics.DurationMs.ToString(CultureInfo.InvariantCulture),
            MaxMemoryUsed = metrics.MaxMemoryUsedMB.ToString(CultureInfo.InvariantCulture),
            MemorySize = metrics.MemorySizeMB.ToString(CultureInfo.InvariantCulture),
            RequestId = requestId,
            Timestamp = @event.Timestamp,
        };

        var builder = new StringBuilder()
            .AppendLine()
            .AppendLine()
            .AppendFormat(CultureInfo.InvariantCulture, $"Timestamp: {log.Timestamp:u}")
            .AppendLine()
            .AppendFormat(CultureInfo.InvariantCulture, $"Request ID: {requestId}")
            .AppendLine()
            .AppendFormat(CultureInfo.InvariantCulture, $"Duration: {metrics.DurationMs}")
            .AppendLine()
            .AppendFormat(CultureInfo.InvariantCulture, $"Billed Duration: {metrics.BilledDurationMs}")
            .AppendLine()
            .AppendFormat(CultureInfo.InvariantCulture, $"Memory Size: {metrics.MemorySizeMB}")
            .AppendLine()
            .AppendFormat(CultureInfo.InvariantCulture, $"Max Memory Used: {metrics.MaxMemoryUsedMB}")
            .AppendLine();

        if (metrics.InitDurationMs is { } initDurationMs)
        {
            entry.InitDuration = initDurationMs.ToString(CultureInfo.InvariantCulture);

            builder.AppendFormat(CultureInfo.InvariantCulture, $"Init Duration: {initDurationMs}")
                   .AppendLine();
        }

        if (report.Record.Tracing?.Value is { Length: > 0 } traceValue &&
            traceValue.Split(';') is { Length: > 0 } values)
        {
            const string RootPrefix = "Root=";
            string? root = values.FirstOrDefault((p) => p.StartsWith(RootPrefix, StringComparison.Ordinal));

            if (root is not null)
            {
                entry.TraceId = root[RootPrefix.Length..];

                builder.AppendFormat(CultureInfo.InvariantCulture, $"XRAY TraceId: {entry.TraceId}")
                        .AppendLine();
            }
        }

        parsed = (entry, builder.ToString());
        return true;
    }

    private bool TryParseEventAsText(
        OutputLogEvent @event,
        out (LogEvent Event, string Message) parsed)
    {
        parsed = default;

        const string ReportPrefix = "REPORT ";

        if (!@event.Message.StartsWith(ReportPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        string[] split = @event.Message.Split('\t', StringSplitOptions.RemoveEmptyEntries);
        string requestIdLine = split[0][ReportPrefix.Length..];
        string requestId = requestIdLine.Split(' ')[1];

        if (!Requests.TryGetValue(requestId, out string? name))
        {
            return false;
        }

        var entry = new LogEvent(name)
        {
            RequestId = requestId,
            Timestamp = @event.Timestamp ?? default,
        };

        var builder = new StringBuilder()
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

                    case "XRAY TraceId":
                        entry.TraceId = parts[1];
                        break;

                    default:
                        break;
                }
            }
        }

        parsed = (entry, builder.ToString());
        return true;
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

        public string? InitDuration { get; set; }

        public string? TraceId { get; set; }
    }
}
