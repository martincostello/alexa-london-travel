// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var alexa = require("alexa-app");
var api = require("./src/api");
var responses = require("./src/responses");
var skill = require("./src/skill")

var app = new alexa.app(skill.name);

app.error = skill.onError;
app.pre = skill.preReqest;
app.post = skill.postResponse;

app.launch(skill.onLaunch);

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

skill.setMessages(app);

module.exports = app;
