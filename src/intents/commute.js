// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var lines = require("./../lines");
var responses = require("./../responses");
var skillApi = require("./../skillApi");
var SsmlBuilder = require("ssml-builder");
var sprintf = require("sprintf");
var statusIntent = require("./status");
var verbalizer = require("./../verbalizer");

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

/**
 * Gets the locale associated with the specified request.
 * @param {Object} request - The Alexa skill request.
 * @returns {String} - The ISO 3166-1 associated with the request, e.g. 'en-GB'.
 */
intent.getLocale = function (request) {

  var locale = "en-GB";

  if ("locale" in request.data.request) {
    locale = request.data.request.locale;
  }

  return locale;
};

intent.noFavoritesResponse = function (locale) {
  return locale === "en-US" ? responses.onNoFavoriteLinesUS : responses.onNoFavoriteLinesUK;
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
      .say(responses.onAccountNotLinked)
      .linkAccount();
  }
  else {
    return intent.api.getPreferences(accessToken)
      .then(function (data) {

        if (data === null) {
          response
            .say(responses.onAccountLinkInvalid)
            .linkAccount();
        }
        else {

          var locale = intent.getLocale(request);
          var text;

          if (!data.favoriteLines || data.favoriteLines.length === 0) {
            text = intent.noFavoritesResponse(locale);
            response
              .say(text)
              .card(intent.generateCard(text));
          }
          else {

            var statuses = [];

            for (var i = 0; i < data.favoriteLines.length; i++) {

              var line = data.favoriteLines[i];
              var promise = statusIntent.getLineStatus(line);

              statuses.push(promise);
            }

            return Promise.all(statuses).then(function (statuses) {

              var builder = new SsmlBuilder();

              for (var i = 0; i < statuses.length; i++) {
                var status = statuses[i];
                var rawText = verbalizer.verbalize(status.text);
                var displayName = lines.toSpokenName(status.name, true);
                builder.paragraph(sprintf("%s: %s", displayName, verbalizer.verbalize(rawText)));
              }

              var text = statusIntent.normalizeTextForCard(builder.ssml(true));

              response
                .say(text)
                .card(intent.generateCard(text));
            })
            .catch(function (err) {
              console.error("Failed to get commute:", err);
              response.say(responses.onError);
            });
          }
        }
      })
      .catch(function (err) {
        console.error("Failed to get commute:", err);
        response.say(responses.onError);
      });
  }
};

module.exports = intent;
