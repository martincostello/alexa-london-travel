// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var cards = require("./../cards");
var responses = require("./../responses");
var SsmlBuilder = require("ssml-builder");
var telemetry = require("../telemetry");
var tflApi = require("./../tflApi");
var verbalizer = require("./../verbalizer");

var intent = {
  api: tflApi,
  name: "DisruptionIntent",
  enabled: true,
  slots: {},
  utterances: [
    "about {|any} {|line|tube} {closures|disruption|disruptions} {|today}",
    "are there any {|line|tube} closures {|today}",
    "if there {are|is} {|any} {|line|tube} {closures|disruption|disruptions} {|today}"
  ]
};

/**
 * Generates the raw status text to respond to the specified disruption response.
 * @param {Object[]} data - An array of disruptions.
 * @returns {String[]} An array containing the raw text response for the specified disruptions, if any.
 */
intent.generateRawResponse = function (data) {

  var statuses = [];

  if (data && data.length > 0) {
    // Deduplicate any status descriptions. For example, if a tube
    // line has a planned closure and severe delays, the message will appear twice.
    for (var i = 0; i < data.length; i++) {
      var description = data[i].description;
      if (statuses.indexOf(description) === -1) {
        statuses.push(description);
      }
    }
  }

  return statuses.sort();
};

/**
 * Generates the text to respond to the specified disruption status(es).
 * @param {String[]} statuses - An array of disruption descriptions.
 * @returns {String} The SSML response for the specified status(es).
 */
intent.generateResponse = function (statuses) {

  var builder = new SsmlBuilder();

  if (!statuses || statuses.length === 0) {
    builder.say(responses.onNoDisruption);
  }
  else {

    for (var i = 0; i < statuses.length; i++) {
      builder.paragraph(verbalizer.verbalize(statuses[i]));
    }

    builder.paragraph("There is a good service on all other lines.");
  }

  return builder.ssml(true);
};

/**
 * Handles the intent for disruption.
 * @param {Object} request - The Alexa skill request.
 * @param {Object} response - The Alexa skill response.
 * @returns {Object} The result of the intent handler.
 */
intent.handler = function (request, response) {

  telemetry.trackEvent(intent.name, {
    sessionId: request.sessionId,
    userId: request.userId
  });

  return intent.api.getDisruption(["dlr", "overground", "tube"])
    .then(function (data) {

      var statuses = intent.generateRawResponse(data);
      var ssml = intent.generateResponse(statuses);
      var card = cards.forDisruption(statuses);

      response
        .say(ssml)
        .card(card);
    })
    .catch(function (err) {
      console.error("Failed to check for disruption:", err);
      response.say(responses.onError);
    });
};

module.exports = intent;
