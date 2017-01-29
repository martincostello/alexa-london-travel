// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var responses = require("./../responses");

var intent = {
  name: "StatusIntent",
  enabled: true,
  slots: {
    "LINE": "LITERAL"
  },
  utterances: [
    "what is the status of the {LINE} {line|}"
  ],
  handler: function (request, response) {
    var line = request.slot("LINE");
    if (line) {
      response
        .say("I'm sorry, I can't tell you the status of " + line + " at the moment.")
        .reprompt(responses.onUnknown);
    } else {
      response.say(responses.onUnknown);
    }
  }
};

module.exports = intent;
