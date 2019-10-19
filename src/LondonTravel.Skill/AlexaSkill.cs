// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Core;

namespace MartinCostello.LondonTravel.Skill
{
    /// <summary>
    /// A class representing the AWS Lambda handler for the London Travel Amazon Alexa skill.
    /// </summary>
    public class AlexaSkill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlexaSkill"/> class.
        /// </summary>
        public AlexaSkill()
            : this(Environment.GetEnvironmentVariable("SKILL_API_HOSTNAME"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlexaSkill"/> class with the specified configuration.
        /// </summary>
        /// <param name="skillApiHostName">The URL of the skill's API.</param>
        internal AlexaSkill(string skillApiHostName)
        {
            SkillApiUrl = skillApiHostName ?? "https://londontravel.martincostello.com/";
        }

        /// <summary>
        /// Gets the URL of the skill's API.
        /// </summary>
        private string SkillApiUrl { get; }

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
            context.Logger.Log($"Invoking skill request of type {input.GetType().Name}.");
            context.Logger.Log($"Skill API URL: {SkillApiUrl}");

            return await Task.FromResult(new SkillResponse());
        }
    }
}
