// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var api = require("./../api");
var responses = require("./../responses");

var intent = {
  name: "TestIntent",
  enabled: false,
  slots: {},
  utterances: [
    "to test connectivity"
  ],
  handler: function (request, response) {
    api.getLineStatus("victoria")
      .then(function (data) {
        var success = data.length && data.length > 0;
        response
          .say(success ? "Test successful." : "Test failed.")
          .card({
            type: "Standard",
            title: "London Travel Test",
            text: success ? "The London Travel skill appears to be working correctly." : "The London Travel skill appears to be experiencing problems at the moment."
          })
          .send();
      })
      .catch(function (err) {
        console.error("Failed to test connectivity: ", err);
        response.say(responses.onError);
      });
    return false;
  }
};

module.exports = intent;
