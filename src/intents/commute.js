// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var responses = require("./../responses");
var skillApi = require("./../skillApi");

var intent = {
  api: skillApi,
  name: "CommuteIntent",
  enabled: true,
  slots: {},
  utterances: [
    "{|about} my commute {|now|right now|this afternoon|this evening|this morning|today|tonight}",
    "what {|is} my commute {|is} like {|now|today|tonight}"
  ]
};

/**
 * Generates the card to respond to the intent.
 * @param {String} text - The text to include in the card.
 * @returns {Object} The card object to use.
 */
intent.generateCard = function (text) {
  return {
    type: "Standard",
    title: "My Commute",
    text: text
  };
};

intent.getLocale = function (request) {

  var locale = "en-GB";

  if ("locale" in request.data.request) {
    locale = request.data.request.locale;
  }

  return locale;

};

intent.noFavoritesResponse = function (locale) {
  return "You have not selected any " + (locale === "en-US" ? "favorite" : "favourite") + " lines yet.";
};

intent.favoritesResponse = function (favoriteLines, locale) {
  return "Your " + (locale === "en-US" ? "favorite" : "favourite") + " lines are: " + favoriteLines.join();
};

/**
 * Handles the intent for disruption.
 * @param {Object} request - The Alexa skill request.
 * @param {Object} response - The Alexa skill response.
 * @returns {Object} The result of the intent handler, if any.
 */
intent.handler = function (request, response) {

  var accessToken = process.env.SKILL_API_ACCESS_TOKEN || "";

  if (!accessToken && request.hasSession() === true) {
    var session = request.getSession();
    accessToken = session.details.accessToken;
  }

  if (!accessToken) {
    response
      .say("You need to link your account to be able to ask me about your commute.")
      .linkAccount();
  }
  else {
    return intent.api.getPreferences(accessToken)
      .then(function (data) {

        if (data === null) {
          response
            .say("It looks like you've disabled account linking. You need to re-link your account to be able to ask me about your commute.")
            .linkAccount();
        }
        else {

          var text;
          var locale = intent.getLocale(request);

          if (!data.favoriteLines || data.favoriteLines.length === 0) {
            text = intent.noFavoritesResponse(locale);
          }
          else {
            text = intent.favoritesResponse(data.favoriteLines, locale);
          }

          var card = intent.generateCard(text);

          response
            .say(text)
            .card(card);
        }
      })
      .catch(function (err) {
        console.error("Failed to get commute:", err);
        response.say(responses.onError);
      });
  }
};

module.exports = intent;
