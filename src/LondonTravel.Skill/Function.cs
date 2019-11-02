// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Reflection;
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
    internal static class Function
    {
        /// <summary>
        /// Runs the function using a custom runtime as an asynchronous operation.
        /// </summary>
        /// <param name="httpClient">The optional HTTP client to use.</param>
        /// <param name="cancellationToken">The optional cancellation token to use.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to run the function.
        /// </returns>
        internal static async Task RunAsync(
            HttpClient httpClient = null,
            CancellationToken cancellationToken = default)
        {
            var serializer = new JsonSerializer();
            var function = new AlexaFunction();

            using var handlerWrapper = HandlerWrapper.GetHandlerWrapper<SkillRequest, SkillResponse>(function.HandlerAsync, serializer);
            using var bootstrap = new LambdaBootstrap(handlerWrapper);

            if (httpClient != null)
            {
                SetHttpClient(bootstrap, httpClient);
            }

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

        private static void SetHttpClient(LambdaBootstrap bootstrap, HttpClient httpClient)
        {
            // Replace the internal runtime API client with one using the specified HttpClient.
            // See https://github.com/aws/aws-lambda-dotnet/blob/4f9142b95b376bd238bce6be43f4e1ec1f983592/Libraries/src/Amazon.Lambda.RuntimeSupport/Bootstrap/LambdaBootstrap.cs#L41
            var client = new RuntimeApiClient(httpClient);

            var property = typeof(LambdaBootstrap).GetProperty("Client", BindingFlags.Instance | BindingFlags.NonPublic);
            property.SetValue(bootstrap, client);
        }
    }
}
