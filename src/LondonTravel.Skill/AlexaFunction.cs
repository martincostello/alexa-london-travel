// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Threading;
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

            VerifySkillId(request, serviceProvider.GetRequiredService<SkillConfiguration>());

            LambdaContextAccessor contextAccessor = serviceProvider.GetRequiredService<LambdaContextAccessor>();
            contextAccessor.LambdaContext = context;

            CultureInfo previousCulture = SetLocale(request);

            SkillResponse response;

            try
            {
                var skill = serviceProvider.GetRequiredService<AlexaSkill>();

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
            }
            finally
            {
                contextAccessor.LambdaContext = null;
                Thread.CurrentThread.CurrentCulture = previousCulture;
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
        /// <returns>
        /// The <see cref="IServiceProvider"/> to use.
        /// </returns>
        private IServiceProvider CreateServiceProvider()
        {
            IServiceCollection services = ServiceResolver.GetServiceCollection(ConfigureServices);

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Sets the locale to use for resources for the current request.
        /// </summary>
        /// <param name="request">The skill request to use to set the locale.</param>
        /// <returns>
        /// The previous locale.
        /// </returns>
        private CultureInfo SetLocale(SkillRequest request)
        {
            var previousCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(request.Request.Locale ?? "en-GB");
            }
#pragma warning disable CA1031
            catch (ArgumentException)
#pragma warning restore CA1031
            {
                // Ignore invalid/unknown cultures
            }

            return previousCulture;
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
