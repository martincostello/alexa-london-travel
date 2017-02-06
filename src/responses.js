// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var Speech = require("ssml-builder");

var responses = {
  noAudioPlayer: "Sorry, this application does not support audio streams.",
  noIntent: "Sorry, I don't understand how to do that.",
  noSession: "Sorry, the session is not available.",
  onError: "Sorry, something went wrong.",
  onInvalidRequest: "Sorry, that request is not valid.",
  onLaunch: "Welcome to London Travel. You can ask me about disruption or for the status of any tube line, London Overground or the D.L.R..",
  onNoDisruption: "There is currently no disruption on the tube, London Overground or the D.L.R.",
  onUnknown: "Sorry, I didn't catch that. You can ask if there is any disruption or about the status of any tube line, London Overground or the D.L.R..",
  onUnknownLine: "Sorry, I am not sure what line you said. You can ask about the status of any tube line, London Overground or the D.L.R..",
  onSessionEnded: "Goodbye.",
  toSsml: function (text) {
    var builder = new Speech();
    builder.say(text);
    return builder.ssml(true);
  }
};

module.exports = responses;
