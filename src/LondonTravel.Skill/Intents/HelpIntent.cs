// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;

namespace MartinCostello.LondonTravel.Skill.Intents
{
    /// <summary>
    /// A class that handles the <c>AMAZON.HelpIntent</c> intent. This class cannot be inherited.
    /// </summary>
    internal sealed class HelpIntent : IIntent
    {
        /// <inheritdoc />
        public Task<SkillResponse> RespondAsync(Intent intent, Session session)
        {
            string[] paragraphs = new[]
            {
                "This skill allows you to check for the status of a specific line, or for disruption in general. You can ask about any London Underground line, London Overground, the Docklands Light Railway or TfL Rail.",
                "Asking about disruption in general provides information about any lines that are currently experiencing issues, such as any delays or planned closures.",
                "Asking for the status for a specific line provides a summary of the current service, such as whether there is a good service or if there are any delays.",
                "If you link your account and setup your preferences in the London Travel website, you can ask about your commute to quickly find out the status of the lines you frequently use.",
            };

            var result = SkillResponseBuilder
                .Tell(paragraphs)
                .ShouldNotEndSession()
                .Build();

            return Task.FromResult(result);
        }
    }
}
