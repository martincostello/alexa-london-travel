// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var api = require("./../api");
var responses = require("./../responses");

var intent = {
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
 * Generates the text to respond to the specified disruption response.
 * @param {Object[]} data - An array of disruptions.
 * @returns {String} The text response for the specified disuptions.
 */
intent.generateResponse = function (data) {

  if (!data || data.length === 0) {
    return responses.onNoDisruption;
  }

  var statuses = [];

  // Deduplicate any status descriptions. For example, if a tube
  // line has a planned closure and severe delays, the message will appear twice.
  for (var i = 0; i < data.length; i++) {
    var description = data[i].description;
    if (statuses.indexOf(description) === -1) {
      statuses.push(description);
    }
  }

  var text = statuses.join("\n");

  return responses.toSsml(text);
};

/**
 * Handles the intent for disruption.
 * @param {Object} request - The Alexa skill request.
 * @param {Object} response - The Alexa skill response.
 * @returns {Object} The result of the intent handler.
 */
intent.handler = function (request, response) {
  api.getDisruption(["dlr", "overground", "tube"])
    .then(function (data) {
      response
        .say(intent.generateResponse(data))
        .send();
    })
    .catch(function (err) {
      console.error("Failed to check for disruption:", err);
      response.say(responses.onError);
    });
  return false;
};

module.exports = intent;
