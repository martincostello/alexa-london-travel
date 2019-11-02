// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace MartinCostello.LondonTravel.Skill.Integration
{
    public class LambdaTestServerTests : ITestOutputHelperAccessor
    {
        public LambdaTestServerTests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        public ITestOutputHelper OutputHelper { get; set; }

        [Fact]
        public async Task Function_Can_Process_Requests()
        {
            // Arrange
            void Configure(IServiceCollection services)
            {
                services.AddLogging((builder) => builder.AddXUnit(this));
            }

            using var server = new LambdaTestServer(Configure);
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            await server.StartAsync(cancellationTokenSource.Token);

            ChannelReader<LambdaResponse> reader = await server.EnqueueAsync(
                "my-request-id",
                @"{""Values"": [ 1, 2, 3 ]}");

            // Queue a task to stop the Lambda function as soon as the response is processed
            _ = Task.Run(async () =>
            {
                await reader.WaitToReadAsync(cancellationTokenSource.Token);

                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                }
            });

            using var httpClient = server.CreateHttpClient();

            // Act
            await MyFunctionEntrypoint.RunAsync(httpClient, cancellationTokenSource.Token);

            // Assert
            reader.TryRead(out LambdaResponse response).ShouldBeTrue();

            response.ShouldNotBeNull();
            response.IsSuccessful.ShouldBeTrue();
            response.Content.ShouldNotBeNull();
            Encoding.UTF8.GetString(response.Content).ShouldBe(@"{""Sum"":6}");
        }

        [Fact]
        public async Task Function_Can_Process_Multiple_Requests()
        {
            // Arrange
            void Configure(IServiceCollection services)
            {
                services.AddLogging((builder) => builder.AddXUnit(this));
            }

            using var server = new LambdaTestServer(Configure);
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            await server.StartAsync(cancellationTokenSource.Token);

            var channels = new List<(int expected, ChannelReader<LambdaResponse> reader)>();

            for (int i = 0; i < 10; i++)
            {
                var request = new MyRequest()
                {
                    Values = Enumerable.Range(1, i + 1).ToArray(),
                };

                string requestId = i.ToString(CultureInfo.InvariantCulture);
                string json = System.Text.Json.JsonSerializer.Serialize(request);

                channels.Add((request.Values.Sum(), await server.EnqueueAsync(requestId, json)));
            }

            // Queue a task to stop the Lambda function as soon as all the responses are processed
            _ = Task.Run(async () =>
            {
                foreach ((var _, var reader) in channels)
                {
                    await reader.WaitToReadAsync(cancellationTokenSource.Token);
                }

                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                }
            });

            using var httpClient = server.CreateHttpClient();

            // Act
            await MyFunctionEntrypoint.RunAsync(httpClient, cancellationTokenSource.Token);

            // Assert
            foreach ((int expected, ChannelReader<LambdaResponse> channel) in channels)
            {
                channel.TryRead(out LambdaResponse response).ShouldBeTrue();

                response.ShouldNotBeNull();
                response.IsSuccessful.ShouldBeTrue();
                response.Content.ShouldNotBeNull();

                var deserialized = System.Text.Json.JsonSerializer.Deserialize<MyResponse>(response.Content);
                deserialized.Sum.ShouldBe(expected);
            }
        }

        private static class MyFunctionEntrypoint
        {
            internal static async Task Main()
                => await RunAsync();

            internal static async Task RunAsync(
                HttpClient httpClient = null,
                CancellationToken cancellationToken = default)
            {
                var handler = new MyHandler();
                var serializer = new JsonSerializer();

                using var handlerWrapper = HandlerWrapper.GetHandlerWrapper<MyRequest, MyResponse>(handler.SumAsync, serializer);
                using var bootstrap = new LambdaBootstrap(handlerWrapper, handler.InitializeAsync);

                if (httpClient != null)
                {
                    // Replace the internal runtime API client with one using the specified HttpClient.
                    // See https://github.com/aws/aws-lambda-dotnet/blob/4f9142b95b376bd238bce6be43f4e1ec1f983592/Libraries/src/Amazon.Lambda.RuntimeSupport/Bootstrap/LambdaBootstrap.cs#L41
                    var client = new RuntimeApiClient(httpClient);

                    var property = typeof(LambdaBootstrap).GetProperty("Client", BindingFlags.Instance | BindingFlags.NonPublic);
                    property.SetValue(bootstrap, client);
                }

                await bootstrap.RunAsync(cancellationToken);
            }
        }

        private sealed class MyHandler
        {
            public Task<bool> InitializeAsync()
            {
                return Task.FromResult(true);
            }

            public Task<MyResponse> SumAsync(MyRequest request, ILambdaContext context)
            {
                context.Logger.LogLine($"Handling AWS request Id {context.AwsRequestId}.");

                var response = new MyResponse()
                {
                    Sum = request.Values?.Sum() ?? 0,
                };

                context.Logger.LogLine($"The sum of the {request.Values?.Count} values is {response.Sum}.");

                return Task.FromResult(response);
            }
        }

        private sealed class MyRequest
        {
            public ICollection<int> Values { get; set; }
        }

        private sealed class MyResponse
        {
            public int Sum { get; set; }
        }
    }
}
