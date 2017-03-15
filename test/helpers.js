// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var buildRequest = function (requestType) {
  return {
    "version": "1.0",
    "session": {
      "new": true,
      "sessionId": "amzn1.echo-api.session.abeee1a7-aee0-41e6-8192-e6faaed9f5ef",
      "application": {
        "applicationId": null
      },
      "attributes": {},
      "user": {
        "userId": "amzn1.account.AM3B227HF3FAM1B261HK7FFM3A2"
      }
    },
    "context": {
      "System": {
        "application": {
          "applicationId": null
        },
        "user": {
          "userId": "amzn1.account.AM3B227HF3FAM1B261HK7FFM3A2"
        },
        "device": {
          "supportedInterfaces": {
            "AudioPlayer": {}
          }
        }
      },
      "AudioPlayer": {
        "offsetInMilliseconds": 0,
        "playerActivity": "IDLE"
      }
    },
    "request": {
      "type": requestType,
      "requestId": "amzn1.echo-api.request.9cdaa4db-f20e-4c58-8d01-c75322d6c423",
      "timestamp": "2015-05-13T12:34:56Z"
    }
  };
};

var helpers = {
  launchRequest: function () {
    return buildRequest("LaunchRequest");
  },
  intentRequest: function (intent, slots, accessToken) {

    var json = buildRequest("IntentRequest");

    json.request.intent = {
      name: intent,
      slots: slots || {}
    };

    if (accessToken !== null) {
      json.session.user.accessToken = accessToken;
    }

    return json;
  },
  sessionEndRequest: function () {
    return buildRequest("SessionEndedRequest");
  }
};

helpers.commuteRequest = function (accessToken) {
  return helpers.intentRequest("CommuteIntent", {}, accessToken);
};

module.exports = helpers;
