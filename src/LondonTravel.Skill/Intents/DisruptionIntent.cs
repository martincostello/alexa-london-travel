// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.Ssml;
using MartinCostello.LondonTravel.Skill.Clients;

namespace MartinCostello.LondonTravel.Skill.Intents
{
    /// <summary>
    /// A class that handles the disruption intent. This class cannot be inherited.
    /// </summary>
    internal sealed class DisruptionIntent : IIntent
    {
        /// <summary>
        /// The supported modes of transport. This field is read-only.
        /// </summary>
        private static readonly string SupportedModes = string.Join(",", "dlr", "overground", "tube", "tflrail");

        /// <summary>
        /// Initializes a new instance of the <see cref="DisruptionIntent"/> class.
        /// </summary>
        /// <param name="tflClient">The TfL API client to use.</param>
        /// <param name="config">The skill configuration to use.</param>
        public DisruptionIntent(ITflClient tflClient, SkillConfiguration config)
        {
            Config = config;
            TflClient = tflClient;
        }

        /// <summary>
        /// Gets the skill configuration.
        /// </summary>
        private SkillConfiguration Config { get; }

        /// <summary>
        /// Gets the TfL API client.
        /// </summary>
        private ITflClient TflClient { get; }

        /// <inheritdoc />
        public async Task<SkillResponse> RespondAsync(Intent intent, Session session)
        {
            ICollection<string> disruptions = await GetRawDisruptionsAsync();

            var elements = new List<ISsml>();
            string cardContent;

            if (disruptions.Count > 0)
            {
                foreach (var disruption in disruptions)
                {
                    string text = Verbalizer.Verbalize(disruption);
                    elements.Add(new Paragraph(new PlainText(text)));
                }

                elements.Add(new Paragraph(new PlainText("There is a good service on all other lines.")));
                cardContent = string.Join("\n", disruptions);
            }
            else
            {
                string text = "There is currently no disruption on the tube, London Overground, the DLR or TfL Rail.";
                string verbalized = Verbalizer.Verbalize(text);

                elements.Add(new PlainText(verbalized));
                cardContent = text;
            }

            cardContent = cardContent
                .Replace("D.L.R.", "DLR", StringComparison.Ordinal)
                .Replace("T.F.L. Rail", "TfL Rail", StringComparison.Ordinal);

            var speech = new Speech(elements.ToArray());

            var response = ResponseBuilder.Tell(speech);

            response.Response.Card = new StandardCard()
            {
                Title = "Disruption Summary",
                Content = cardContent,
            };

            return response;
        }

        private async Task<ICollection<ServiceDisruption>> GetDisruptionAsync()
        {
            return await TflClient.GetDisruptionAsync(
                SupportedModes,
                Config.TflApplicationId,
                Config.TflApplicationKey);
        }

        private async Task<IList<string>> GetRawDisruptionsAsync()
        {
            ICollection<ServiceDisruption> disruptions = await GetDisruptionAsync();

            var descriptions = new List<string>();

            foreach (var disruption in disruptions)
            {
                descriptions.Add(disruption.Description);
            }

            // Deduplicate any status descriptions. For example, if a tube line
            // has a planned closure and severe delays, the message will appear twice.
            return descriptions
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy((p) => p, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
