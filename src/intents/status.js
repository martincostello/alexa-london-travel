// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var api = require("./../api");
var responses = require("./../responses");

var mapLineToId = function (line) {

  var normalized = line.toLowerCase();

  switch (normalized) {

    case "bakerloo":
    case "central":
    case "circle":
    case "district":
    case "jubilee":
    case "metropolitan":
    case "northern":
    case "piccadilly":
    case "victoria":
      return normalized;

    case "hammersmith and city":
      return "hammersmith-city";

    case "waterloo and city":
      return "waterloo-city";

    default:
      return null;
  }
}

var generateResponse = function (data) {
  if (data.length > 0) {
    var line = data[0];
    if (line.lineStatuses.length > 0) {
      var status = line.lineStatuses[0];
      switch (status.statusSeverity) {
        case 10:
          return "There is currently a good service on the " + line.name + " line.";

        case 20:
          return "The " + line.name + " line is currently closed.";

        default:
          return "There is currently disruption on the " + line.name + " line.";
      }
    }
  }
  return responses.onError;
};

var intent = {
  name: "StatusIntent",
  enabled: true,
  slots: {
    "LINE": "LINE_NAME"
  },
  utterances: [
    "{-|LINE} status",
    "{|what is the} status of the {-|LINE} {|line}"
  ],
  handler: function (request, response) {
    var line = mapLineToId(request.slot("LINE"));
    if (line) {
      api.getLineStatus(line)
        .then(function (data) {
          response
            .say(generateResponse(data))
            .send();
        });
      return false;
    } else {
      response.say(responses.onUnknown);
    }
  }
};

module.exports = intent;
