// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var alexa = require("alexa-app");
var skill = require("./src/skill");
var telemetry = require("./src/telemetry");

// Setup the telemetry client
telemetry.setup(process.env.APPINSIGHTS_INSTRUMENTATIONKEY);

// Create the application for the skill
var app = new alexa.app(skill.name);

// Set the skill dictionary
app.dictionary = skill.dictionary;

// Register the generic handlers
app.error = skill.onError;
app.pre = skill.preReqest;
app.post = skill.postResponse;

// Register the launch and session end handlers
app.launch(skill.onLaunch);
app.sessionEnded(skill.onSessionEnded);

// Register the enabled intents
for (var i = 0; i < skill.intents.length; i++) {
  var intent = skill.intents[i];
  app.intent(
    intent.name,
    {
      slots: intent.slots,
      utterances: intent.utterances
    },
    intent.handler
  );
}

// Apply any message customizations
skill.setMessages(app);

module.exports = app;
