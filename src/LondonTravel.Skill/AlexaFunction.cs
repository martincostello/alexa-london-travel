// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.LondonTravel.Skill
{
    /// <summary>
    /// A class representing the AWS Lambda function entry-point for the London Travel Amazon Alexa skill.
    /// </summary>
    public class AlexaFunction
    {
        /// <summary>
        /// Handles a request to the skill as an asynchronous operation.
        /// </summary>
        /// <param name="request">The skill request.</param>
        /// <param name="context">The AWS Lambda execution context.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the skill's response.
        /// </returns>
        public async Task<SkillResponse> HandlerAsync(SkillRequest request, ILambdaContext context)
        {
            context.Logger.LogLine($"Invoking skill request of type {request.Request.GetType().Name}.");

            IServiceProvider serviceProvider = CreateServiceProvider();

            var handler = serviceProvider.GetRequiredService<FunctionHandler>();
            var accessor = serviceProvider.GetRequiredService<LambdaContextAccessor>();

            accessor.LambdaContext = context;

            try
            {
                return await handler.HandleAsync(request);
            }
            finally
            {
                accessor.LambdaContext = null;
            }
        }

        /// <summary>
        /// Configures the <see cref="IServiceCollection"/> to use.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // No-op
        }

        /// <summary>
        /// Creates the <see cref="IServiceProvider"/> to use.
        /// </summary>
        /// <returns>
        /// The <see cref="IServiceProvider"/> to use.
        /// </returns>
        private IServiceProvider CreateServiceProvider()
        {
            return ServiceResolver.GetServiceCollection(ConfigureServices).BuildServiceProvider();
        }
    }
}
