// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill.Integration
{
    /// <summary>
    /// A class representing a test AWS Lambda runtime HTTP server for an AWS Lambda function.
    /// </summary>
    public class TestLambdaServer : IDisposable
    {
        private readonly Action<IServiceCollection> _configure;
        private readonly CancellationTokenSource _onDisposed;

        private bool _disposed;
        private RuntimeHandler _handler;
        private TestServer _server;
        private CancellationTokenSource _onStopped;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestLambdaServer"/> class.
        /// </summary>
        public TestLambdaServer()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestLambdaServer"/> class.
        /// </summary>
        /// <param name="configure">An optional delegate to invoke when configuring the test Lambda runtime server.</param>
        public TestLambdaServer(Action<IServiceCollection> configure)
        {
#pragma warning disable CA1308
            FunctionName = "test-function";
            FunctionArn = $"arn:aws:lambda:eu-west-1:123456789012:function:{FunctionName.ToLowerInvariant()}";
#pragma warning restore CA1308

            _configure = configure;
            _onDisposed = new CancellationTokenSource();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="TestLambdaServer"/> class.
        /// </summary>
        ~TestLambdaServer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets or sets the ARN of the Lambda function being tested.
        /// </summary>
        public string FunctionArn { get; set; }

        /// <summary>
        /// Gets or sets the name of the Lambda function being tested.
        /// </summary>
        public string FunctionName { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates an <see cref="HttpClient"/> to use to interact with the test Lambda runtime server.
        /// </summary>
        /// <returns>
        /// An <see cref="HttpClient"/> that can be used to process Lambda runtime HTTP requests.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The test server has not been started.
        /// </exception>
        public HttpClient CreateHttpClient()
        {
            if (_server == null)
            {
                throw new InvalidOperationException("The test server has not been started.");
            }

            return _server.CreateClient();
        }

        /// <summary>
        /// Enqueues a request for the Lambda function to process as an asynchronous operation.
        /// </summary>
        /// <param name="awsRequestId">The AWS request Id associated with the content.</param>
        /// <param name="content">The request content to process.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to enqueue the request
        /// which returns a channel reader which completes once the request is processed by the function.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="awsRequestId"/> or <paramref name="content"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// A request with Id <paramref name="awsRequestId"/> is already in-flight or the test server has not been started.
        /// </exception>
        public async Task<ChannelReader<LambdaResponse>> EnqueueAsync(string awsRequestId, byte[] content)
        {
            if (awsRequestId == null)
            {
                throw new ArgumentNullException(nameof(awsRequestId));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (_server == null)
            {
                throw new InvalidOperationException("The test server has not been started.");
            }

            return await _handler.EnqueueAsync(awsRequestId, content, _onStopped.Token);
        }

        /// <summary>
        /// Starts the test Lambda runtime server as an asynchronous operation.
        /// </summary>
        /// <param name="cancellationToken">
        /// The optional cancellation token to use to signal the server should stop listening to invocation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to start the test Lambda runtime server.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The test server has already been started.
        /// </exception>
        public virtual Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_server != null)
            {
                throw new InvalidOperationException("The test server has already been started.");
            }

            _onStopped = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _onDisposed.Token);
            _handler = new RuntimeHandler(FunctionArn, _onStopped.Token);

            var builder = new WebHostBuilder();

            ConfigureWebHost(builder);

            _server = new TestServer(builder);

            _handler.Logger = _server.Services.GetRequiredService<ILogger<RuntimeHandler>>();

            SetLambdaEnvironmentVariables(_server.BaseAddress);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources;
        /// <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_onDisposed != null)
                    {
                        // The token for _onStopped is linked to this token, so this will cancel both
                        if (!_onDisposed.IsCancellationRequested)
                        {
                            _onDisposed.Cancel();
                        }

                        _onDisposed.Dispose();
                        _onStopped?.Dispose();
                    }

                    _server?.Dispose();

                    ClearLambdaEnvironmentVariables();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Configures the application for the test Lambda runtime server.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to configure.</param>
        protected virtual void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints((endpoints) =>
            {
                endpoints.MapGet("/{LambdaVersion}/runtime/invocation/next", _handler.HandleNextAsync);
                endpoints.MapPost("/{LambdaVersion}/runtime/init/error", _handler.HandleInitializationErrorAsync);
                endpoints.MapPost("/{LambdaVersion}/runtime/invocation/{AwsRequestId}/error", _handler.HandleInvocationErrorAsync);
                endpoints.MapPost("/{LambdaVersion}/runtime/invocation/{AwsRequestId}/response", _handler.HandleResponseAsync);
            });
        }

        /// <summary>
        /// Configures the services for the test Lambda runtime server application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to use.</param>
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            _configure?.Invoke(services);
        }

        /// <summary>
        /// Configures the web host builder for the test Lambda runtime server.
        /// </summary>
        /// <param name="builder">The <see cref="IWebHostBuilder"/> to configure.</param>
        protected virtual void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(Environment.CurrentDirectory);

            builder.ConfigureServices(ConfigureServices);
            builder.Configure(Configure);
        }

        private static void ClearLambdaEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("AWS_LAMBDA_RUNTIME_API", null);
        }

        private static void SetLambdaEnvironmentVariables(Uri baseAddress)
        {
            Environment.SetEnvironmentVariable("AWS_LAMBDA_RUNTIME_API", $"{baseAddress.Host}:{baseAddress.Port}");
        }
    }
}
