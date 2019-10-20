// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.Ssml;
using Amazon.Lambda.Core;

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
        /// <param name="context">The <see cref="ILambdaContext"/> to use.</param>
        public UnknownIntent(ILambdaContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Gets the current AWS Lambda request context.
        /// </summary>
        private ILambdaContext Context { get; }

        /// <inheritdoc />
        public Task<SkillResponse> RespondAsync(Intent intent, Session session)
        {
            Context.Logger.LogLine($"Unknown intent '{intent.Name}' cannot be handled.");

            var plaintext = new PlainText("Sorry, I don't understand how to do that.");
            var speech = new Speech(plaintext);

            return Task.FromResult(ResponseBuilder.Tell(speech));
        }
    }
}
