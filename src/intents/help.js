// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var Speech = require("ssml-builder");
var telemetry = require("../telemetry");

var intent = {
  name: "AMAZON.HelpIntent",
  enabled: true
};

var generateResponse = function () {

  var builder = new Speech();

  builder
    .paragraph("This skill allows you to check for the status of a specific line, or for disruption in general. You can ask about any London Underground line, London Overground, the Docklands Light Railway or TfL Rail.")
    .paragraph("Asking about disruption in general provides information about any lines that are currently experiencing issues, such as any delays or planned closures.")
    .paragraph("Asking for the status for a specific line provides a summary of the current service, such as whether there is a good service or if there are any delays.")
    .paragraph("If you link your account and setup your preferences in the London Travel website, you can ask about your commute to quickly find out the status of the lines you frequently use.");

  return builder.ssml(true);
};

/**
 * Handles the intent for stop.
 * @param {Object} request - The Alexa skill request.
 * @param {Object} response - The Alexa skill response.
 */
intent.handler = function (request, response) {

  telemetry.trackEvent(intent.name, {
    sessionId: request.sessionId,
    userId: request.userId
  });

  var text = generateResponse();
  response
    .say(text)
    .shouldEndSession(false);
};

module.exports = intent;
