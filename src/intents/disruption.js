// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var api = require("./../api");
var responses = require("./../responses");

var generateResponse = function (data) {

  if (data.length === 0) {
    return "There is currently no disruption on the tube or the Docklands Light Railway."
  }

  // TODO For now just return the first disruption
  return data[0].description;
};

var intent = {
  name: "DisruptionIntent",
  enabled: true,
  slots: {},
  utterances: [
    "are there any {|line|tube} closures {|today}",
    "if there {are|is} {|any} {closures|disruption} {|today}"
  ],
  handler: function (request, response) {
    api.getDisruption(["dlr", "tube"])
      .then(function (data) {
        response
          .say(generateResponse(data))
          .send();
      })
      .catch(function (err) {
        response.say(responses.onError);
      });
    return false;
  }
};

module.exports = intent;
