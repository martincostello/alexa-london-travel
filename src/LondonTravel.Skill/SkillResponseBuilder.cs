// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Xml.Linq;
using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill;

internal sealed class SkillResponseBuilder
{
    private SkillResponseBuilder(SpeechXml speech)
    {
        Speech = speech;
        Response = new()
        {
            Response = new()
            {
                OutputSpeech = new()
                {
                    Ssml = speech.ToXml(),
                },
            },
        };
    }

    private Card? Card { get; set; }

    private SkillResponse Response { get; }

    private bool ShouldEndSession { get; set; } = true;

    private SpeechXml Speech { get; }

    public static SkillResponseBuilder Tell(params string[] paragraphs)
    {
        return Tell(paragraphs as ICollection<string>);
    }

    public static SkillResponseBuilder Tell(ICollection<string> paragraphs)
    {
        ArgumentNullException.ThrowIfNull(paragraphs);

        var rawText = new List<Ssml>(paragraphs.Count);

        foreach (var text in paragraphs.Select(Verbalizer.Verbalize).Select((p) => new PlainText(p)))
        {
            rawText.Add(text);
        }

        var elements = new List<Ssml>(rawText.Count);

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

        var speech = new SpeechXml(elements);

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
        Response.Response.Reprompt = new Reprompt()
        {
            OutputSpeech = new()
            {
                Ssml = Speech.ToXml(),
            },
        };
        return this;
    }

    private sealed class Paragraph(Ssml element) : Ssml
    {
        public override XNode ToXml()
            => new XElement("p", [element.ToXml()]);
    }

    private sealed class PlainText(string text) : Ssml
    {
        public override XNode ToXml()
            => new XText(text);
    }

    private sealed class SpeechXml(List<Ssml> elements)
    {
        public string ToXml()
        {
            Debug.Assert(elements.Count > 0, "No text available.");

            var root = new XElement(
                "speak",
                new XAttribute(XNamespace.Xmlns + "amazon", "http://alexa.amazon.com"),
                new XAttribute(XNamespace.Xmlns + "alexa", "http://alexaactual.amazon.com"));

            root.Add(elements.Select((p) => p.ToXml()));

            string xmlString = root.ToString(SaveOptions.DisableFormatting);

            const string SpeakTag = "<speak>";

            if (xmlString.StartsWith(SpeakTag, StringComparison.Ordinal))
            {
                return xmlString;
            }

            int endOfSpeakTag = xmlString.IndexOf('>', StringComparison.Ordinal);
            return string.Concat(SpeakTag, xmlString.AsSpan(endOfSpeakTag + 1));
        }
    }

    private abstract class Ssml
    {
        public abstract XNode ToXml();
    }
}
