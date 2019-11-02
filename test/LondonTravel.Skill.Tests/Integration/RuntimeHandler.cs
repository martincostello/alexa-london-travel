// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill.Integration
{
    /// <summary>
    /// A class representing a handler for AWS Lambda runtime HTTP requests. This class cannot be inherited.
    /// </summary>
    internal sealed class RuntimeHandler
    {
        /// <summary>
        /// The cancellation token that is signalled when request listening should stop. This field is read-only.
        /// </summary>
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// The test server's options. This field is read-only.
        /// </summary>
        private readonly LambdaTestServerOptions _options;

        /// <summary>
        /// The channel of function requests to process. This field is read-only.
        /// </summary>
        private readonly Channel<LambdaRequest> _requests;

        /// <summary>
        /// A dictionary containing channels for the responses for enqueued requests. This field is read-only.
        /// </summary>
        private readonly ConcurrentDictionary<string, Channel<LambdaResponse>> _responses;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeHandler"/> class.
        /// </summary>
        /// <param name="options">The test server's options.</param>
        /// <param name="cancellationToken">The cancellation token that is signalled when request listening should stop.</param>
        internal RuntimeHandler(LambdaTestServerOptions options, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _options = options;

            // Support multi-threaded access to the request queue, although the default
            // usage scenario would be a single reader and writer from a test method.
            var channelOptions = new UnboundedChannelOptions()
            {
                SingleReader = false,
                SingleWriter = false,
            };

            _requests = Channel.CreateUnbounded<LambdaRequest>(channelOptions);
            _responses = new ConcurrentDictionary<string, Channel<LambdaResponse>>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Gets or sets the logger to use.
        /// </summary>
        internal ILogger Logger { get; set; }

        /// <summary>
        /// Enqueues a request for the Lambda function to process as an asynchronous operation.
        /// </summary>
        /// <param name="awsRequestId">The AWS request Id associated with the content.</param>
        /// <param name="content">The request content to process.</param>
        /// <param name="cancellationToken">The cancellation token to use when enqueuing the item.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to enqueue the request
        /// which returns a channel reader which completes once the request is processed by the function.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// A request with Id <paramref name="awsRequestId"/> is already in-flight.
        /// </exception>
        internal async Task<ChannelReader<LambdaResponse>> EnqueueAsync(
            string awsRequestId,
            byte[] content,
            CancellationToken cancellationToken)
        {
            // There is only one response per request, so the channel is bounded to one item
            var channel = Channel.CreateBounded<LambdaResponse>(1);

            if (!_responses.TryAdd(awsRequestId, channel))
            {
                throw new InvalidOperationException($"A request with AWS request Id '{awsRequestId}' is already in-flight.");
            }

            // Enqueue the request for the Lambda runtime to process
            var item = new LambdaRequest(awsRequestId, content);
            await _requests.Writer.WriteAsync(item, cancellationToken);

            // Return the reader to the caller to await the function being handled
            return channel.Reader;
        }

        /// <summary>
        /// Handles a request for the next invocation for the Lambda function.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to get the next invocation request.
        /// </returns>
        internal async Task HandleNextAsync(HttpContext httpContext)
        {
            Logger.LogInformation(
                "Waiting for new request for Lambda function with ARN {LambdaFunctionArn}.",
                _options);

            LambdaRequest request;

            try
            {
                // Additionally cancel the listen loop if the processing is stopped
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(httpContext.RequestAborted, _cancellationToken);

                // Wait until there is a request to process
                await _requests.Reader.WaitToReadAsync(cts.Token);
                request = await _requests.Reader.ReadAsync();
            }
            catch (OperationCanceledException ex)
            {
                Logger.LogInformation(
                    ex,
                    "Stopped listening for additional requests for Lambda function with ARN {LambdaFunctionArn}.",
                    _options);

                // Send a dummy response to prevent the listen loop from erroring
                request = new LambdaRequest("completed", new[] { (byte)'{', (byte)'}' });

                // This dummy request wasn't enqueued, so it needs manually adding
                _responses.GetOrAdd(request.AwsRequestId, (_) => Channel.CreateBounded<LambdaResponse>(1));
            }

            // Write the response for the Lambda runtime to pass to the function to invoke
            string traceId = Guid.NewGuid().ToString();

            Logger.LogInformation(
                "Invoking Lambda function with ARN {LambdaFunctionArn} for request Id {AwsRequestId} and trace Id {AwsTraceId}.",
                _options,
                request.AwsRequestId,
                traceId);

            _responses.GetOrAdd(request.AwsRequestId, (_) => Channel.CreateBounded<LambdaResponse>(1));

            // These headers are required, as otherwise an exception is thrown
            httpContext.Response.Headers.Add("Lambda-Runtime-Aws-Request-Id", request.AwsRequestId);
            httpContext.Response.Headers.Add("Lambda-Runtime-Invoked-Function-Arn", _options.FunctionArn);

            var deadline = DateTimeOffset.UtcNow.Add(_options.FunctionTimeout).ToUnixTimeMilliseconds();

            httpContext.Response.Headers.Add("Lambda-Runtime-Deadline-Ms", deadline.ToString("F0", CultureInfo.InvariantCulture));
            httpContext.Response.Headers.Add("Lambda-Runtime-Trace-Id", traceId);

            httpContext.Response.ContentType = MediaTypeNames.Application.Json;
            httpContext.Response.StatusCode = StatusCodes.Status200OK;

            await httpContext.Response.BodyWriter.WriteAsync(request.Content, httpContext.RequestAborted);
        }

        /// <summary>
        /// Handles an successful response for an invocation of the Lambda function.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to handle the response.
        /// </returns>
        internal async Task HandleResponseAsync(HttpContext httpContext)
        {
            string awsRequestId = httpContext.Request.RouteValues["AwsRequestId"] as string;

            byte[] content = await ReadContentAsync(httpContext, httpContext.RequestAborted);

            Logger.LogInformation(
                "Invoked Lambda function with ARN {LambdaFunctionArn} for request Id {AwsRequestId}: {ResponseContent}.",
                _options,
                awsRequestId,
                ToString(content));

            await CompleteRequestChannelAsync(
                awsRequestId,
                content,
                isSuccessful: true,
                httpContext.RequestAborted);

            httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
        }

        /// <summary>
        /// Handles an error response for an invocation of the Lambda function.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to handle the response.
        /// </returns>
        internal async Task HandleInvocationErrorAsync(HttpContext httpContext)
        {
            string awsRequestId = httpContext.Request.RouteValues["AwsRequestId"] as string;

            byte[] content = await ReadContentAsync(httpContext, httpContext.RequestAborted);

            Logger.LogError(
                "Error invoking Lambda function with ARN {LambdaFunctionArn} for request Id {AwsRequestId}: {ErrorContent}",
                _options,
                awsRequestId,
                ToString(content));

            await CompleteRequestChannelAsync(
                awsRequestId,
                content,
                isSuccessful: false,
                httpContext.RequestAborted);

            httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
        }

        /// <summary>
        /// Handles an error response for the failed initialization of the Lambda function.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to handle the response.
        /// </returns>
        internal async Task HandleInitializationErrorAsync(HttpContext httpContext)
        {
            byte[] content = await ReadContentAsync(httpContext, httpContext.RequestAborted);

            Logger.LogError(
                "Error initializing Lambda function with ARN {LambdaFunctionArn}: {ErrorContent}",
                _options,
                ToString(content));

            httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
        }

        private static async Task<byte[]> ReadContentAsync(HttpContext httpContext, CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream();

            await httpContext.Request.BodyReader.CopyToAsync(stream, cancellationToken);

            return stream.ToArray();
        }

        private static string ToString(byte[] content)
        {
            return Encoding.UTF8.GetString(content);
        }

        private async Task CompleteRequestChannelAsync(
            string awsRequestId,
            byte[] content,
            bool isSuccessful,
            CancellationToken cancellationToken)
        {
            if (!_responses.TryRemove(awsRequestId, out Channel<LambdaResponse> channel))
            {
                Logger.LogError(
                    "Could not find response channel with AWS request Id {AwsRequestId} for Lambda function with ARN {LambdaFunctionArn}.",
                    awsRequestId,
                    _options);

                return;
            }

            // Make the response available to read by the enqueuer
            var response = new LambdaResponse(content, isSuccessful);
            await channel.Writer.WriteAsync(response, cancellationToken);

            // Mark the channel as complete as there will be no more responses written
            channel.Writer.Complete();
        }

        private sealed class LambdaRequest
        {
            internal LambdaRequest(string awsRequestId, byte[] content)
            {
                AwsRequestId = awsRequestId;
                Content = content;
            }

            public string AwsRequestId { get; }

            public byte[] Content { get; }
        }
    }
}
