// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Buffers;
using System.Runtime.CompilerServices;
using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill;

internal sealed class SkillResponseBuilder
{
    private const string SpeakStart = "<speak>";
    private const string SpeakEnd = "</speak>";
    private const string ParagraphStart = "<p>";
    private const string ParagraphEnd = "</p>";

    /// <summary>
    /// The characters that need to be escaped or normalized in SSML text content. This field is read-only.
    /// </summary>
    private static readonly SearchValues<char> CharactersToEscape = SearchValues.Create("&<>\r");

    private readonly string _ssml;

    private SkillResponseBuilder(string ssml)
    {
        _ssml = ssml;
        Response = new()
        {
            Response = new()
            {
                OutputSpeech = new()
                {
                    Ssml = ssml,
                },
            },
        };
    }

    private Card? Card { get; set; }

    private SkillResponse Response { get; }

    private bool ShouldEndSession { get; set; } = true;

    public static SkillResponseBuilder Tell(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return new(ToSsml(text));
    }

    public static SkillResponseBuilder Tell(IReadOnlyList<string> paragraphs)
    {
        ArgumentNullException.ThrowIfNull(paragraphs);
        return new(ToSsml(paragraphs));
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
                Ssml = _ssml,
            },
        };

        return this;
    }

    private static string ToSsml(string text)
    {
        string verbalized = Verbalizer.Verbalize(text);

        if (!verbalized.AsSpan().ContainsAny(CharactersToEscape))
        {
            // Leave as just a simple sentence
            return string.Concat(SpeakStart, verbalized, SpeakEnd);
        }

        var builder = new DefaultInterpolatedStringHandler(SpeakStart.Length + verbalized.Length + SpeakEnd.Length, 0);

        builder.AppendLiteral(SpeakStart);
        AppendEscaped(ref builder, verbalized);
        builder.AppendLiteral(SpeakEnd);

        return builder.ToStringAndClear();
    }

    private static string ToSsml(IReadOnlyList<string> paragraphs)
    {
        if (paragraphs.Count == 1)
        {
            return ToSsml(paragraphs[0]);
        }

        int length = SpeakStart.Length + SpeakEnd.Length;

        for (int i = 0; i < paragraphs.Count; i++)
        {
            length += ParagraphStart.Length + paragraphs[i].Length + ParagraphEnd.Length;
        }

        var builder = new DefaultInterpolatedStringHandler(length, 0);

        builder.AppendLiteral(SpeakStart);

        for (int i = 0; i < paragraphs.Count; i++)
        {
            builder.AppendLiteral(ParagraphStart);
            AppendEscaped(ref builder, Verbalizer.Verbalize(paragraphs[i]));
            builder.AppendLiteral(ParagraphEnd);
        }

        builder.AppendLiteral(SpeakEnd);

        return builder.ToStringAndClear();
    }

    private static void AppendEscaped(ref DefaultInterpolatedStringHandler builder, ReadOnlySpan<char> text)
    {
        while (!text.IsEmpty)
        {
            int index = text.IndexOfAny(CharactersToEscape);

            if (index < 0)
            {
                builder.AppendFormatted(text);
                return;
            }

            builder.AppendFormatted(text[..index]);

            char value = text[index];
            text = text[(index + 1)..];

            switch (value)
            {
                case '&':
                    builder.AppendLiteral("&amp;");
                    break;

                case '<':
                    builder.AppendLiteral("&lt;");
                    break;

                case '>':
                    builder.AppendLiteral("&gt;");
                    break;

                default:
                    if (text.IsEmpty || text[0] != '\n')
                    {
                        builder.AppendLiteral("\n");
                    }

                    break;
            }
        }
    }
}
