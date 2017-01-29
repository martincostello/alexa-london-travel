// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var constants = require("./constants");
var responses = require("./responses");
var statusIntent = require("./intents/status");
var testIntent = require("./intents/test");

var skill = {
  name: constants.appName,
  onError: function (exception, request, response) {
    console.log("Unhandled exception: ", exception);
    response.say(responses.onError);
  },
  onLaunch: function (request, response) {
    response.say(responses.onLaunch);
  },
  onSessionEnded: function (request, response) {
    response.say(responses.onSessionEnded);
  },
  preReqest: function (request, response, type) {
  },
  postResponse: function (request, response, type, exception) {
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
    statusIntent,
    testIntent
  ]
};

module.exports = skill;
