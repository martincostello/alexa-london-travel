// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var alexa = require("alexa-app");
var app = new alexa.app("alexa-london-travel");
var http = require("request-promise");

app.launch(function (request, response) {
  response.say("Welcome to the London Travel skill.");
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
        .reprompt("Sorry, I didn't catch that.");
    } else {
      response.say("Sorry, I didn't catch that.");
    }
  }
);

app.intent("TestIntent", {
  "slots": [],
  "utterances": ["to test connectivity"]
},
  function (request, response) {
    var options = {
      uri: "https://api.tfl.gov.uk/Line/victoria/Status",
      qs: {
        app_id: process.env.TFL_APP_ID || "",
        app_key: process.env.TFL_APP_KEY || ""
      },
      headers: {
        "User-Agent": "alexa-london-travel/0.0.1"
      },
      json: true
    };
    http(options)
      .then(function (data) {
        response
          .say(data.length && data.length > 0 ? "Test successful." : "Test failed.")
          .send();
      })
      .catch(function (err) {
        console.log("Failed to test connectivity: ", err);
        response.say("Sorry, an error occurred.");
      });
    return false;
  }
);

app.error = function (exception, request, response) {
  console.log("Unhandled exception: ", exception);
  response.say("Sorry, an error occurred.");
};

app.pre = function (request, response, type) {
};

app.post = function (request, response, type, exception) {
};

app.messages.NO_INTENT_FOUND = "Sorry, I don't understand how to do that.";

module.exports = app;
