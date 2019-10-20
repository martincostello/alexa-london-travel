// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;

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
        /// <param name="contextAccessor">The <see cref="LambdaContextAccessor"/> to use.</param>
        public UnknownIntent(LambdaContextAccessor contextAccessor)
        {
            ContextAccessor = contextAccessor;
        }

        /// <summary>
        /// Gets the AWS Lambda request context accessor.
        /// </summary>
        private LambdaContextAccessor ContextAccessor { get; }

        /// <inheritdoc />
        public Task<SkillResponse> RespondAsync(Intent intent, Session session)
        {
            ContextAccessor.LambdaContext.Logger.LogLine($"Unknown intent '{intent.Name}' cannot be handled.");

            var response = SkillResponseBuilder
                .Tell("Sorry, I don't understand how to do that.")
                .Build();

            return Task.FromResult(response);
        }
    }
}
