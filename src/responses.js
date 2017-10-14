// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var SsmlBuilder = require("ssml-builder");
var verbalizer = require("./verbalizer");

var responses = {
  noAudioPlayer: "Sorry, this application does not support audio streams.",
  noIntent: "Sorry, I don't understand how to do that.",
  noSession: "Sorry, the session is not available.",
  onError: "Sorry, something went wrong.",
  onInvalidRequest: "Sorry, that request is not valid.",
  onAccountNotLinked: "You need to link your account to be able to ask me about your commute.",
  onAccountLinkInvalid: "It looks like you've disabled account linking. You need to re-link your account to be able to ask me about your commute.",
  onLaunch: verbalizer.verbalize("Welcome to London Travel. You can ask me about disruption or for the status of any tube line, London Overground, the DLR or TfL Rail."),
  onNoDisruption: verbalizer.verbalize("There is currently no disruption on the tube, London Overground, the DLR or TfL Rail."),
  onNoFavoriteLinesUK: "You have not selected any favourite lines yet. Visit the London Travel website to set your preferences.",
  onNoFavoriteLinesUS: "You have not selected any favorite lines yet. Visit the London Travel website to set your preferences.",
  onUnknownLine: verbalizer.verbalize("Sorry, I am not sure what line you said. You can ask about the status of any tube line, London Overground, the DLR or TfL Rail."),
  onElizabethLine: "Sorry, I cannot tell you about the status of the Elizabeth Line yet.",
  onSessionEnded: "Goodbye.",
  toSsml: function (text) {
    var builder = new SsmlBuilder();
    builder.say(text);
    return builder.ssml(true);
  }
};

module.exports = responses;
