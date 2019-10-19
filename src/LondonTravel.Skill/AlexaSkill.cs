// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.Ssml;
using Amazon.Lambda.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace MartinCostello.LondonTravel.Skill
{
    /// <summary>
    /// A class representing the implementation of the London Travel Alexa skill.
    /// </summary>
    public class AlexaSkill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlexaSkill"/> class.
        /// </summary>
        /// <param name="context">The AWS Lambda execution context.</param>
        /// <param name="config">The skill configuration to use.</param>
        public AlexaSkill(ILambdaContext context, SkillConfiguration config)
        {
            Context = context;
            Config = config;

#pragma warning disable CA2000
            Telemetry = new TelemetryClient(TelemetryConfiguration.CreateDefault())
            {
                InstrumentationKey = config.ApplicationInsightsKey,
            };
#pragma warning restore CA2000
        }

        /// <summary>
        /// Gets the AWS Lambda execution context.
        /// </summary>
        private ILambdaContext Context { get; }

        /// <summary>
        /// Gets the skill configuration.
        /// </summary>
        private SkillConfiguration Config { get; }

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

            return ResponseBuilder.Tell("Sorry, something went wrong.");
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
            TrackEvent(intent.Name, session);

            IIntent userIntent = IntentFactory.Create(intent);

            if (userIntent is Intents.UnknownIntent)
            {
                Context.Logger.LogLine($"Unknown intent '{intent.Name}' cannot be handled.");
            }

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

            string text = Verbalizer.Verbalize(
                "Welcome to London Travel. You can ask me about disruption or for the status of any tube line, London Overground, the DLR or TfL Rail.");

            var plaintext = new PlainText(text);
            var speech = new Speech(plaintext);

            var result = ResponseBuilder.Tell(speech);

            result.Response.ShouldEndSession = false;

            return result;
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

            var plaintext = new PlainText("Goodbye.");
            var speech = new Speech(plaintext);

            return ResponseBuilder.Tell(speech);
        }

        private IDictionary<string, string> ToTelemetryProperties(Session session)
        {
            return new Dictionary<string, string>()
            {
                ["sessionId"] = session.SessionId,
                ["userId"] = session.User?.UserId,
            };
        }

        private void TrackEvent(string eventName, Session session)
        {
            IDictionary<string, string> properties = ToTelemetryProperties(session);
            Telemetry.TrackEvent(eventName, properties);
        }

        private void TrackException(Exception exception, Session session)
        {
            IDictionary<string, string> properties = ToTelemetryProperties(session);
            Telemetry.TrackException(exception, properties);
        }
    }
}
