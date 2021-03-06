// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;

namespace MartinCostello.LondonTravel.Skill
{
    /// <summary>
    /// A class representing the entry-point to a custom AWS Lambda runtime. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// See https://aws.amazon.com/blogs/developer/announcing-amazon-lambda-runtimesupport/.
    /// </remarks>
    public static class FunctionEntrypoint
    {
        /// <summary>
        /// Runs the function using a custom runtime as an asynchronous operation.
        /// </summary>
        /// <param name="httpClient">The optional HTTP client to use.</param>
        /// <param name="cancellationToken">The optional cancellation token to use.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to run the function.
        /// </returns>
        public static async Task RunAsync(
            HttpClient httpClient = null,
            CancellationToken cancellationToken = default)
        {
            var serializer = new JsonSerializer();
            var function = new AlexaFunction();

#pragma warning disable CA2000
            using var handlerWrapper = HandlerWrapper.GetHandlerWrapper<SkillRequest, SkillResponse>(function.HandlerAsync, serializer);
            using var bootstrap = new LambdaBootstrap(httpClient ?? new HttpClient(), handlerWrapper, function.InitializeAsync);
#pragma warning restore CA2000

            await bootstrap.RunAsync(cancellationToken);
        }

        /// <summary>
        /// The main entry point for the custom runtime.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to run the custom runtime.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private static async Task Main() => await RunAsync();
    }
}
