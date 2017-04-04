// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var constants = require("./constants");
var responses = require("./responses");
var cancelIntent = require("./intents/cancel");
var commuteIntent = require("./intents/commute");
var disruptionIntent = require("./intents/disruption");
var helpIntent = require("./intents/help");
var statusIntent = require("./intents/status");
var stopIntent = require("./intents/stop");
var telemetry = require("./telemetry");

var skill = {
  dictionary: {
    "LINE_NAMES": [
      "Bakerloo",
      "Central",
      "Circle",
      "District",
      "DLR",
      "Hammersmith and City",
      "Jubilee",
      "London Overground",
      "Metropolitan",
      "Northern",
      "Piccadilly",
      "Victoria",
      "Waterloo and City"
    ]
  },
  name: constants.appName,
  onError: function (exception, request, response) {

    console.error("Unhandled exception: ", exception);

    telemetry.trackException(exception, {
      sessionId: request.sessionId,
      userId: request.userId
    });

    response.say(responses.onError);
  },
  onLaunch: function (request, response) {

    telemetry.trackEvent("LaunchRequest", {
      sessionId: request.sessionId,
      userId: request.userId
    });

    response
      .say(responses.onLaunch)
      .shouldEndSession(false);
  },
  onSessionEnded: function (request, response) {

    telemetry.trackEvent("SessionEndedRequest", {
      sessionId: request.sessionId,
      userId: request.userId
    });

    response.say(responses.onSessionEnded);
  },
  preReqest: function (request, response, type) {

    console.info("Handling request.", JSON.stringify({
      request: request.data.request,
      sessionId: request.sessionId,
      userId: request.userId
    }));

    if (process.env.VERIFY_SKILL_ID && request.applicationId !== (process.env.SKILL_ID || null)) {
      console.error("Request application Id and configured skill Id mismatch.", request.applicationId, process.env.SKILL_ID);
      response.fail("Invalid application Id.");
    }
  },
  postResponse: function (request, response, type, exception) {
    console.info("Handled request.", JSON.stringify({
      request: request.data.request,
      sessionId: request.sessionId,
      userId: request.userId,
      exception: exception
    }));
  },
  setMessages: function (app) {
    app.messages.GENERIC_ERROR = responses.onError;
    app.messages.INVALID_REQUEST_TYPE = responses.onInvalidRequest;
    app.messages.NO_AUDIO_PLAYER_EVENT_HANDLER_FOUND = responses.noAudioPlayer;
    app.messages.NO_INTENT_FOUND = responses.noIntent;
    app.messages.NO_LAUNCH_FUNCTION = responses.noIntent;
    app.messages.NO_SESSION = responses.noSession;
  },
  intents: [
    cancelIntent,
    commuteIntent,
    disruptionIntent,
    helpIntent,
    statusIntent,
    stopIntent
  ]
};

module.exports = skill;
