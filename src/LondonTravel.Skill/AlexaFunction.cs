// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;

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
            : this(SkillConfiguration.CreateDefaultConfiguration())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlexaFunction"/> class with the specified configuration.
        /// </summary>
        /// <param name="config">The <see cref="SkillConfiguration"/> to use.</param>
        public AlexaFunction(SkillConfiguration config)
        {
            Config = config;
        }

        /// <summary>
        /// Gets the skill configuration.
        /// </summary>
        private SkillConfiguration Config { get; }

        /// <summary>
        /// Handles a request to the skill as an asynchronous operation.
        /// </summary>
        /// <param name="input">The skill request.</param>
        /// <param name="context">The AWS Lambda execution context.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the skill's response.
        /// </returns>
        public async Task<SkillResponse> HandlerAsync(SkillRequest input, ILambdaContext context)
        {
            context.Logger.LogLine($"Invoking skill request of type {input.GetType().Name}.");

            VerifySkillId(input);

            var skill = new AlexaSkill(context, Config);

            SkillResponse response;

            try
            {
                if (input.Request is LaunchRequest launch)
                {
                    response = skill.OnLaunch(input.Session);
                }
                else if (input.Request is IntentRequest intent)
                {
                    response = await skill.OnIntentAsync(intent.Intent, input.Session);
                }
                else if (input.Request is SessionEndedRequest)
                {
                    response = skill.OnSessionEnded(input.Session);
                }
                else
                {
                    response = skill.OnError(null, input.Session);
                }
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                response = skill.OnError(ex, input.Session);
            }

            return response;
        }

        /// <summary>
        /// Verifies the skill Id.
        /// </summary>
        /// <param name="input">The function input.</param>
        private void VerifySkillId(SkillRequest input)
        {
            if (Config.VerifySkillId)
            {
                string applicationId = input.Session.Application.ApplicationId;

                if (!string.Equals(applicationId, Config.SkillId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Request application Id '{applicationId}' and configured skill Id '{Config.SkillId}' mismatch.");
                }
            }
        }
    }
}
