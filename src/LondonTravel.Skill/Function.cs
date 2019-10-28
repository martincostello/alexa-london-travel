// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;

namespace MartinCostello.LondonTravel.Skill
{
    /// <summary>
    /// A class representing the entry-point to a custom AWS Lambda runtime. This class cannot be inherited.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class Function
    {
        /// <summary>
        /// The main entry point for the custom runtime.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to run the custom runtime.
        /// </returns>
        /// <remarks>
        /// See https://aws.amazon.com/blogs/developer/announcing-amazon-lambda-runtimesupport/.
        /// </remarks>
        internal static async Task Main()
        {
            var serializer = new JsonSerializer();
            var function = new AlexaFunction();

            Func<SkillRequest, ILambdaContext, Task<SkillResponse>> handler = function.HandlerAsync;

            using var handlerWrapper = HandlerWrapper.GetHandlerWrapper(handler, serializer);
            using var bootstrap = new LambdaBootstrap(handlerWrapper);

            await bootstrap.RunAsync();
        }
    }
}
