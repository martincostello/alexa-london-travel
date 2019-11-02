// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using MartinCostello.Testing.AwsLambdaTestServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace MartinCostello.LondonTravel.Skill
{
    public class EndToEndTests : FunctionTests
    {
        public EndToEndTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async Task Alexa_Function_Can_Process_Request()
        {
            // Arrange
            SkillRequest request = CreateRequest<LaunchRequest>();
            request.Request.Type = "LaunchRequest";

            string json = JsonConvert.SerializeObject(request);

            void Configure(IServiceCollection services)
            {
                services.AddLogging((builder) => builder.AddXUnit(this));
            }

            using var server = new LambdaTestServer(Configure);
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            await server.StartAsync(cancellationTokenSource.Token);

            ChannelReader<LambdaTestResponse> reader = await server.EnqueueAsync(json);

            // Queue a task to stop the Lambda function as soon as the response is processed
            _ = Task.Run(async () =>
            {
                await reader.WaitToReadAsync(cancellationTokenSource.Token);

                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                }
            });

            using var httpClient = server.CreateClient();

            // Act
            await Function.RunAsync(httpClient, cancellationTokenSource.Token);

            // Assert
            reader.TryRead(out LambdaTestResponse result).ShouldBeTrue();

            result.ShouldNotBeNull();
            result.IsSuccessful.ShouldBeTrue();
            result.Content.ShouldNotBeNull();
            result.Content.ShouldNotBeEmpty();

            json = System.Text.Encoding.UTF8.GetString(result.Content);
            var actual = JsonConvert.DeserializeObject<SkillResponse>(json);

            actual.ShouldNotBeNull();

            ResponseBody response = AssertResponse(actual, shouldEndSession: false);

            response.Card.ShouldBeNull();
            response.Reprompt.ShouldBeNull();

            response.OutputSpeech.ShouldNotBeNull();
            response.OutputSpeech.Type.ShouldBe("SSML");

            var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
            ssml.Ssml.ShouldBe("<speak>Welcome to London Travel. You can ask me about disruption or for the status of any tube line, London Overground, the D.L.R. or T.F.L. Rail.</speak>");
        }
    }
}
