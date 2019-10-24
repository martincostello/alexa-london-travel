// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

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
        /// <param name="telemetry">The telemetry client to use.</param>
        /// <param name="logger">The logger to use.</param>
        public AlexaSkill(
            IntentFactory intentFactory,
            TelemetryClient telemetry,
            ILogger<AlexaSkill> logger)
        {
            IntentFactory = intentFactory;
            Logger = logger;
            Telemetry = telemetry;
        }

        /// <summary>
        /// Gets the AWS Lambda execution context.
        /// </summary>
        private IntentFactory IntentFactory { get; }

        /// <summary>
        /// Gets the logger to use.
        /// </summary>
        private ILogger Logger { get; }

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
            Logger.LogError(exception, "Failed to handle request for session {SessionId}.", session.SessionId);

            TrackException(exception, session);

            return SkillResponseBuilder
                .Tell(Strings.InternalError)
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
                .Tell(Strings.LaunchResponse)
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
                .Tell(Strings.SessionEndResponse)
                .Build();
        }

        private IDictionary<string, string> ToTelemetryProperties(Session session)
        {
            bool hasAccessToken = !string.IsNullOrEmpty(session.User?.AccessToken);

            return new Dictionary<string, string>()
            {
                ["hasAccessToken"] = hasAccessToken.ToString(CultureInfo.InvariantCulture),
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
