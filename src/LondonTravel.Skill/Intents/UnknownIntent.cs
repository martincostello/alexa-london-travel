// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using Alexa.NET.Response;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill.Intents
{
    /// <summary>
    /// A class that handles an unknown intent. This class cannot be inherited.
    /// </summary>
    internal sealed class UnknownIntent : IIntent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownIntent"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public UnknownIntent(ILogger<UnknownIntent> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Gets the logger to use.
        /// </summary>
        private ILogger Logger { get; }

        /// <inheritdoc />
        public Task<SkillResponse> RespondAsync(Intent intent, Session session)
        {
            Logger.LogWarning(
                "Unknown intent {IntentName} cannot be handled for session Id {SessionId}.",
                intent.Name,
                session.SessionId);

            var response = SkillResponseBuilder
                .Tell(Strings.UnknownCommand)
                .Build();

            return Task.FromResult(response);
        }
    }
}
