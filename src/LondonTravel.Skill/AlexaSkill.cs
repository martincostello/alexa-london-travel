// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Microsoft.ApplicationInsights;

namespace MartinCostello.LondonTravel.Skill
{
    /// <summary>
    /// A class representing the implementation of the London Travel Alexa skill. This class cannot be inherited.
    /// </summary>
    internal sealed class AlexaSkill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlexaSkill"/> class.
        /// </summary>
        /// <param name="intentFactory">The factory to use for the skill intents.</param>
        /// <param name="context">The AWS Lambda execution context.</param>
        /// <param name="telemetry">The telemetry client to use.</param>
        public AlexaSkill(
            IntentFactory intentFactory,
            ILambdaContext context,
            TelemetryClient telemetry)
        {
            Context = context;
            IntentFactory = intentFactory;
            Telemetry = telemetry;
        }

        /// <summary>
        /// Gets the AWS Lambda execution context.
        /// </summary>
        private ILambdaContext Context { get; }

        /// <summary>
        /// Gets the AWS Lambda execution context.
        /// </summary>
        private IntentFactory IntentFactory { get; }

        /// <summary>
        /// Gets the telemetery client to use.
        /// </summary>
        private TelemetryClient Telemetry { get; }

        /// <summary>
        /// Handles an error.
        /// </summary>
        /// <param name="exception">The exception that occured, if any.</param>
        /// <param name="session">The Alexa session.</param>
        /// <returns>
        /// The <see cref="ResponseBody"/> to return from the skill.
        /// </returns>
        public SkillResponse OnError(Exception exception, Session session)
        {
            Context.Logger.LogLine($"Failed to handle request: {exception}");

            TrackException(exception, session);

            return SkillResponseBuilder
                .Tell("Sorry, something went wrong.")
                .Build();
        }

        /// <summary>
        /// Handles the skill intent as an asynchronous operation.
        /// </summary>
        /// <param name="intent">The intent to handle.</param>
        /// <param name="session">The Alexa session.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation
        /// which returns the <see cref="ResponseBody"/> to return from the skill.
        /// </returns>
        public async Task<SkillResponse> OnIntentAsync(Intent intent, Session session)
        {
            TrackEvent(intent.Name, session, intent);

            IIntent userIntent = IntentFactory.Create(intent);

            return await userIntent.RespondAsync(intent, session);
        }

        /// <summary>
        /// Handles the skill being launched.
        /// </summary>
        /// <param name="session">The Alexa session.</param>
        /// <returns>
        /// The <see cref="ResponseBody"/> to return from the skill.
        /// </returns>
        public SkillResponse OnLaunch(Session session)
        {
            TrackEvent("LaunchRequest", session);

            return SkillResponseBuilder
                .Tell("Welcome to London Travel. You can ask me about disruption or for the status of any tube line, London Overground, the DLR or TfL Rail.")
                .ShouldNotEndSession()
                .Build();
        }

        /// <summary>
        /// Handles the skill session ending.
        /// </summary>
        /// <param name="session">The Alexa session.</param>
        /// <returns>
        /// The <see cref="ResponseBody"/> to return from the skill.
        /// </returns>
        public SkillResponse OnSessionEnded(Session session)
        {
            TrackEvent("SessionEndedRequest", session);

            return SkillResponseBuilder
                .Tell("Goodbye.")
                .Build();
        }

        private IDictionary<string, string> ToTelemetryProperties(Session session)
        {
            return new Dictionary<string, string>()
            {
                ["sessionId"] = session.SessionId,
                ["userId"] = session.User?.UserId,
            };
        }

        private void TrackEvent(string eventName, Session session, Intent intent = null)
        {
            IDictionary<string, string> properties = ToTelemetryProperties(session);

            if (intent != null)
            {
                foreach (var slot in intent.Slots.Values)
                {
                    properties[$"slot:{slot.Name}"] = slot.Value;
                }
            }

            Telemetry.TrackEvent(eventName, properties);
        }

        private void TrackException(Exception exception, Session session)
        {
            IDictionary<string, string> properties = ToTelemetryProperties(session);
            Telemetry.TrackException(exception, properties);
        }
    }
}
