// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;

namespace MartinCostello.LondonTravel.Skill
{
    /// <summary>
    /// A class representing the function handler for the London Travel Amazon Alexa skill.
    /// </summary>
    internal sealed class FunctionHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionHandler"/> class.
        /// </summary>
        /// <param name="skill">The <see cref="AlexaSkill"/> to use.</param>
        /// <param name="config">The <see cref="SkillConfiguration"/> to use.</param>
        public FunctionHandler(AlexaSkill skill, SkillConfiguration config)
        {
            Config = config;
            Skill = skill;
        }

        /// <summary>
        /// Gets the <see cref="SkillConfiguration"/> to use.
        /// </summary>
        private SkillConfiguration Config { get; }

        /// <summary>
        /// Gets the <see cref="AlexaSkill"/> to use.
        /// </summary>
        private AlexaSkill Skill { get; }

        /// <summary>
        /// Handles a request to the skill as an asynchronous operation.
        /// </summary>
        /// <param name="request">The skill request.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the skill's response.
        /// </returns>
        public async Task<SkillResponse> HandleAsync(SkillRequest request)
        {
            VerifySkillId(request);

            CultureInfo previousCulture = SetLocale(request);

            try
            {
                return await HandleRequestAsync(request);
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }
        }

        /// <summary>
        /// Handles the specified request as an asynchronous operation.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation to handle the request.
        /// </returns>
        private async Task<SkillResponse> HandleRequestAsync(SkillRequest request)
        {
            try
            {
                if (request.Request is LaunchRequest)
                {
                    return Skill.OnLaunch(request.Session);
                }
                else if (request.Request is IntentRequest intent)
                {
                    return await Skill.OnIntentAsync(intent.Intent, request.Session);
                }
                else if (request.Request is SessionEndedRequest)
                {
                    return Skill.OnSessionEnded(request.Session);
                }
                else
                {
                    return Skill.OnError(null, request.Session);
                }
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                return Skill.OnError(ex, request.Session);
            }
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
            var previousCulture = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(request.Request.Locale ?? "en-GB");
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
        private void VerifySkillId(SkillRequest request)
        {
            if (Config.VerifySkillId)
            {
                string applicationId = request.Session.Application.ApplicationId;

                if (!string.Equals(applicationId, Config.SkillId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Request application Id '{applicationId}' and configured skill Id '{Config.SkillId}' mismatch.");
                }
            }
        }
    }
}
