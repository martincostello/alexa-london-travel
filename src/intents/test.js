// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var api = require("./../api");
var responses = require("./../responses");

var intent = {
  name: "TestIntent",
  enabled: true,
  slots: {},
  utterances: [
    "to test connectivity"
  ],
  handler: function (request, response) {
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
};

module.exports = intent;
