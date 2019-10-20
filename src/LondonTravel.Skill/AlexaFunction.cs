// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.LondonTravel.Skill
{
    /// <summary>
    /// A class representing the AWS Lambda function for the London Travel Amazon Alexa skill.
    /// </summary>
    public class AlexaFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlexaFunction"/> class.
        /// </summary>
        public AlexaFunction()
        {
        }

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

            IServiceProvider serviceProvider = CreateServiceProvider(context);

            VerifySkillId(request, serviceProvider.GetRequiredService<SkillConfiguration>());

            var skill = serviceProvider.GetRequiredService<AlexaSkill>();

            SkillResponse response;

            try
            {
                if (request.Request is LaunchRequest)
                {
                    response = skill.OnLaunch(request.Session);
                }
                else if (request.Request is IntentRequest intent)
                {
                    response = await skill.OnIntentAsync(intent.Intent, request.Session);
                }
                else if (request.Request is SessionEndedRequest)
                {
                    response = skill.OnSessionEnded(request.Session);
                }
                else
                {
                    response = skill.OnError(null, request.Session);
                }
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                response = skill.OnError(ex, request.Session);
            }

            return response;
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
        /// <param name="context">The AWS Lambda context to create the service provider with.</param>
        /// <returns>
        /// The <see cref="IServiceProvider"/> to use.
        /// </returns>
        private IServiceProvider CreateServiceProvider(ILambdaContext context)
        {
            IServiceCollection services = ServiceResolver.GetServiceCollection(context, ConfigureServices);

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Verifies the skill Id.
        /// </summary>
        /// <param name="request">The function request.</param>
        /// <param name="config">The skill configuration.</param>
        private void VerifySkillId(SkillRequest request, SkillConfiguration config)
        {
            if (config.VerifySkillId)
            {
                string applicationId = request.Session.Application.ApplicationId;

                if (!string.Equals(applicationId, config.SkillId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Request application Id '{applicationId}' and configured skill Id '{config.SkillId}' mismatch.");
                }
            }
        }
    }
}
