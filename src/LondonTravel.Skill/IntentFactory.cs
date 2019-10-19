// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using MartinCostello.LondonTravel.Skill.Intents;

namespace MartinCostello.LondonTravel.Skill
{
    /// <summary>
    /// A class representing a factory for <see cref="IIntent"/> instances. This class cannot be inherited.
    /// </summary>
    internal static class IntentFactory
    {
        /// <summary>
        /// Creates an intent responder for the specified intent.
        /// </summary>
        /// <param name="intent">The intent to create a responder for.</param>
        /// <returns>
        /// The <see cref="IIntent"/> to use to generate a response for the intent.
        /// </returns>
        internal static IIntent Create(Intent intent)
        {
            switch (intent.Name.ToUpperInvariant())
            {
                case "AMAZON.CancelIntent":
                case "AMAZON.StopIntent":
                    return new EmptyIntent();

                case "AMAZON.HelpIntent":
                    return new HelpIntent();

                case "CommuteIntent":
                    return new CommuteIntent();

                case "DistruptionIntent":
                    return new DisruptionIntent();

                case "StatusIntent":
                    return new StatusIntent();

                default:
                    return new UnknownIntent();
            }
        }
    }
}
