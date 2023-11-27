// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Alexa.NET.Response;

namespace MartinCostello.LondonTravel.Skill;

internal static class SkillResponseExtensions
{
    public static Models.SkillResponse ToModel(this SkillResponse value)
    {
        var model = new Models.SkillResponse()
        {
            Response = new(),
            Version = value.Version,
        };

        if (value.Response.Card is { } card)
        {
            model.Response.Card = card switch
            {
                LinkAccountCard _ => new Models.LinkAccountCard(),
                StandardCard standard => new Models.StandardCard()
                {
                    Content = standard.Content,
                    Title = standard.Title,
                },
                _ => throw new UnreachableException($"Unexpected card type {card.GetType()}."),
            };
        }

        Debug.Assert(
            value.Response.OutputSpeech is null || value.Response.OutputSpeech is SsmlOutputSpeech,
            $"Unexpected output speech type {value.Response.OutputSpeech?.GetType()}");

        if (value.Response.OutputSpeech is SsmlOutputSpeech ssml)
        {
            model.Response.OutputSpeech = new() { Ssml = ssml.Ssml };
        }

        Debug.Assert(
            value.Response.Reprompt is null || value.Response.Reprompt.OutputSpeech is SsmlOutputSpeech,
            $"Unexpected output speech type {value.Response.Reprompt?.OutputSpeech.GetType()}");

        if (value.Response.Reprompt is { } reprompt &&
            reprompt.OutputSpeech is SsmlOutputSpeech speech)
        {
            model.Response.Reprompt = new()
            {
                OutputSpeech = new() { Ssml = speech.Ssml },
            };
        }

        model.Response.ShouldEndSession = value.Response.ShouldEndSession;

        return model;
    }
}
