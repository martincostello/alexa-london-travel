// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var alexa = require("alexa-app");
var api = require("./src/api");
var constants = require("./src/constants");
var responses = require("./src/responses");

var app = new alexa.app(constants.appName);

app.launch(function (request, response) {
  response.say(responses.onLaunch);
});

app.intent("StatusIntent", {
  slots: { "LINE": "LITERAL" },
  utterances: ["what is the status of the {LINE} {line|}"]
},
  function (request, response) {
    var line = request.slot("LINE");
    if (line) {
      response
        .say("I'm sorry, I can't tell you the status of " + line + " at the moment.")
        .reprompt(responses.onUnknown);
    } else {
      response.say(responses.onUnknown);
    }
  }
);

app.intent("TestIntent", {
  slots: [],
  utterances: ["to test connectivity"]
},
  function (request, response) {
    api.getLineStatus("victoria")
      .then(function (data) {
        response
          .say(data.length && data.length > 0 ? "Test successful." : "Test failed.")
          .send();
      })
      .catch(function (err) {
        console.log("Failed to test connectivity: ", err);
        response.say(responses.onError);
      });
    return false;
  }
);

app.error = function (exception, request, response) {
  console.log("Unhandled exception: ", exception);
  response.say(responses.onError);
};

app.pre = function (request, response, type) {
};

app.post = function (request, response, type, exception) {
};

app.messages.NO_INTENT_FOUND = responses.noIntent;

module.exports = app;
