// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var constants = require("./constants");
var responses = require("./responses");

var skill = {
  name: constants.appName,
  intents: [
  ],
  onError: function (exception, request, response) {
    console.log("Unhandled exception: ", exception);
    response.say(responses.onError);
  },
  onLaunch: function (request, response) {
    response.say(responses.onLaunch);
  },
  preReqest: function (request, response, type) {
  },
  postResponse: function (request, response, type, exception) {
  },
  setMessages: function (app) {
    app.messages.NO_INTENT_FOUND = responses.noIntent;
  }
};

module.exports = skill;
