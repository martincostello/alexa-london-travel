// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.Ssml;

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
            var paragraph = new Paragraph();

            paragraph.Elements.Add(new Sentence("This skill allows you to check for the status of a specific line, or for disruption in general. You can ask about any London Underground line, London Overground, the Docklands Light Railway or TfL Rail."));
            paragraph.Elements.Add(new Sentence("Asking about disruption in general provides information about any lines that are currently experiencing issues, such as any delays or planned closures."));
            paragraph.Elements.Add(new Sentence("Asking for the status for a specific line provides a summary of the current service, such as whether there is a good service or if there are any delays."));
            paragraph.Elements.Add(new Sentence("If you link your account and setup your preferences in the London Travel website, you can ask about your commute to quickly find out the status of the lines you frequently use."));

            var speech = new Speech(paragraph);

            var result = ResponseBuilder.Ask(speech, new Reprompt());

            return Task.FromResult(result);
        }
    }
}
