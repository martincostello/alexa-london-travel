// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Response.Ssml;

namespace MartinCostello.LondonTravel.Skill;

internal class SkillResponseBuilder
{
    private SkillResponseBuilder(Speech speech)
    {
        Speech = speech;
        Response = ResponseBuilder.Tell(Speech);
    }

    private ICard Card { get; set; }

    private SkillResponse Response { get; }

    private bool ShouldEndSession { get; set; } = true;

    private Speech Speech { get; }

    public static SkillResponseBuilder Tell(params string[] paragraphs)
    {
        return Tell(paragraphs as IEnumerable<string>);
    }

    public static SkillResponseBuilder Tell(IEnumerable<string> paragraphs)
    {
        var rawText = new List<IParagraphSsml>();

        foreach (string paragraph in paragraphs)
        {
            string text = Verbalizer.Verbalize(paragraph);
            rawText.Add(new PlainText(text));
        }

        var elements = new List<ISsml>();

        if (rawText.Count == 1)
        {
            elements.Add(rawText[0]); // Leave as just a simple sentence
        }
        else
        {
            foreach (var text in rawText)
            {
                elements.Add(new Paragraph(text));
            }
        }

        var speech = new Speech(elements.ToArray());

        return new SkillResponseBuilder(speech);
    }

    public SkillResponse Build()
    {
        Response.Response.Card = Card;
        Response.Response.ShouldEndSession = ShouldEndSession;

        return Response;
    }

    public SkillResponseBuilder LinkAccount()
    {
        Card = new LinkAccountCard();
        return this;
    }

    public SkillResponseBuilder ShouldNotEndSession()
    {
        ShouldEndSession = false;
        return this;
    }

    public SkillResponseBuilder WithCard(string title, string content)
    {
        Card = new StandardCard()
        {
            Title = title,
            Content = content,
        };

        return this;
    }

    public SkillResponseBuilder WithReprompt()
    {
        Response.Response.Reprompt = new Reprompt(Speech);
        return this;
    }
}
