// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using JustEat.HttpClientInterception;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace MartinCostello.LondonTravel.Skill
{
    public class CommuteTests : FunctionTests
    {
        public CommuteTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async Task Can_Invoke_Function_When_The_Skill_Is_Not_Linked()
        {
            // Arrange
            AlexaFunction function = CreateFunction();
            SkillRequest request = CreateIntentRequestWithToken(accessToken: null);
            ILambdaContext context = CreateContext();

            // Act
            SkillResponse actual = await function.HandlerAsync(request, context);

            // Assert
            ResponseBody response = AssertResponse(actual);

            response.Card.ShouldNotBeNull();
            response.Card.ShouldBeOfType<LinkAccountCard>();

            response.Reprompt.ShouldBeNull();

            response.OutputSpeech.ShouldNotBeNull();
            response.OutputSpeech.Type.ShouldBe("SSML");

            var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
            ssml.Ssml.ShouldBe("<speak>You need to link your account to be able to ask me about your commute.</speak>");
        }

        [Fact]
        public async Task Can_Invoke_Function_When_The_Skill_Token_Is_Invalid()
        {
            // Arrange
            Interceptor.RegisterBundle(Path.Combine("Bundles", "skill-api-invalid-token.json"));

            AlexaFunction function = CreateFunction();
            SkillRequest request = CreateIntentRequestWithToken(accessToken: "invalid-access-token");
            ILambdaContext context = CreateContext();

            // Act
            SkillResponse actual = await function.HandlerAsync(request, context);

            // Assert
            ResponseBody response = AssertResponse(actual);

            response.Card.ShouldNotBeNull();
            response.Card.ShouldBeOfType<LinkAccountCard>();

            response.Reprompt.ShouldBeNull();

            response.OutputSpeech.ShouldNotBeNull();
            response.OutputSpeech.Type.ShouldBe("SSML");

            var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
            ssml.Ssml.ShouldBe("<speak>It looks like you've disabled account linking. You need to re-link your account to be able to ask me about your commute.</speak>");
        }

        [Fact]
        public async Task Can_Invoke_Function_When_The_Skill_Api_Fails()
        {
            // Arrange
            AlexaFunction function = CreateFunction();
            SkillRequest request = CreateIntentRequestWithToken(accessToken: "random-access-token");
            ILambdaContext context = CreateContext();

            // Act
            SkillResponse actual = await function.HandlerAsync(request, context);

            // Assert
            ResponseBody response = AssertResponse(actual);

            response.Card.ShouldBeNull();
            response.Reprompt.ShouldBeNull();

            response.OutputSpeech.ShouldNotBeNull();
            response.OutputSpeech.Type.ShouldBe("SSML");

            var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
            ssml.Ssml.ShouldBe("<speak>Sorry, something went wrong.</speak>");
        }

        [Fact]
        public async Task Can_Invoke_Function_When_The_Skill_Is_Linked_And_Has_No_Favorite_Lines()
        {
            // Arrange
            Interceptor.RegisterBundle(Path.Combine("Bundles", "skill-api-no-favorites.json"));
            Interceptor.RegisterBundle(Path.Combine("Bundles", "tfl-line-statuses.json"));

            AlexaFunction function = CreateFunction();
            SkillRequest request = CreateIntentRequestWithToken(accessToken: "token-for-no-favorites");
            ILambdaContext context = CreateContext();

            // Act
            SkillResponse actual = await function.HandlerAsync(request, context);

            // Assert
            AssertResponse(
                actual,
                "<speak>You have not selected any favourite lines yet. Visit the London Travel website to set your preferences.</speak>",
                "You have not selected any favourite lines yet. Visit the London Travel website to set your preferences.");
        }

        [Fact]
        public async Task Can_Invoke_Function_When_The_Skill_Is_Linked_And_Only_The_Elizabeth_Line_Is_A_Favorite()
        {
            // Arrange
            Interceptor.RegisterBundle(Path.Combine("Bundles", "skill-api-elizabeth.json"));
            Interceptor.RegisterBundle(Path.Combine("Bundles", "tfl-line-statuses.json"));

            AlexaFunction function = CreateFunction();
            SkillRequest request = CreateIntentRequestWithToken(accessToken: "token-for-only-elizabeth-line");
            ILambdaContext context = CreateContext();

            // Act
            SkillResponse actual = await function.HandlerAsync(request, context);

            // Assert
            AssertResponse(
                actual,
                "<speak>You have not selected any favourite lines yet. Visit the London Travel website to set your preferences.</speak>",
                "You have not selected any favourite lines yet. Visit the London Travel website to set your preferences.");
        }

        [Fact]
        public async Task Can_Invoke_Function_When_The_Skill_Is_Linked_And_Has_One_Favorite_Line()
        {
            // Arrange
            Interceptor.RegisterBundle(Path.Combine("Bundles", "skill-api-one-favorite.json"));
            Interceptor.RegisterBundle(Path.Combine("Bundles", "tfl-line-statuses.json"));

            AlexaFunction function = CreateFunction();
            SkillRequest request = CreateIntentRequestWithToken(accessToken: "token-for-one-favorite");
            ILambdaContext context = CreateContext();

            // Act
            SkillResponse actual = await function.HandlerAsync(request, context);

            // Assert
            AssertResponse(
                actual,
                "<speak>Saturday 19 and Sunday 20 October, no service between Hammersmith / Wimbledon / Kensington Olympia and South Kensington / Edgware Road. Replacement buses operate.</speak>",
                "Saturday 19 and Sunday 20 October, no service between Hammersmith / Wimbledon / Kensington Olympia and South Kensington / Edgware Road. Replacement buses operate.");
        }

        [Fact]
        public async Task Can_Invoke_Function_When_The_Skill_Is_Linked_And_Has_Two_Favorite_Lines()
        {
            // Arrange
            Interceptor.RegisterBundle(Path.Combine("Bundles", "skill-api-two-favorites.json"));
            Interceptor.RegisterBundle(Path.Combine("Bundles", "tfl-line-statuses.json"));

            AlexaFunction function = CreateFunction();
            SkillRequest request = CreateIntentRequestWithToken(accessToken: "token-for-two-favorites");
            ILambdaContext context = CreateContext();

            // Act
            SkillResponse actual = await function.HandlerAsync(request, context);

            // Assert
            AssertResponse(
                actual,
                "<speak><p>Northern Line: There is a good service on the Northern line.</p><p>Victoria Line: There is a good service on the Victoria line.</p></speak>",
                "Northern Line: There is a good service on the Northern line.\nVictoria Line: There is a good service on the Victoria line.");
        }

        private void AssertResponse(SkillResponse actual, string expectedSsml, string expectedCardContent)
        {
            ResponseBody response = AssertResponse(actual);

            response.Reprompt.ShouldBeNull();

            response.OutputSpeech.ShouldNotBeNull();
            response.OutputSpeech.Type.ShouldBe("SSML");

            var ssml = response.OutputSpeech.ShouldBeOfType<SsmlOutputSpeech>();
            ssml.Ssml.ShouldBe(expectedSsml);

            response.Card.ShouldNotBeNull();
            var card = response.Card.ShouldBeOfType<StandardCard>();

            card.Type.ShouldBe("Standard");
            card.Title.ShouldBe("Your Commute");
            card.Content.ShouldBe(expectedCardContent);
        }

        private SkillRequest CreateIntentRequestWithToken(string accessToken = null, string locale = null)
        {
            var request = CreateIntentRequest("CommuteIntent");

            if (accessToken != null)
            {
                request.Session.User = new User()
                {
                    AccessToken = accessToken,
                    UserId = Guid.NewGuid().ToString(),
                };
            }

            if (locale != null)
            {
                request.Request.Locale = locale;
            }

            return request;
        }
    }
}
