// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var api = require("./../api");
var responses = require("./../responses");

var intent = {
  api: api,
  name: "StatusIntent",
  enabled: true,
  slots: {
    "LINE": "LINE_NAME"
  },
  utterances: [
    "{-|LINE} status",
    "{|what is the} status of the {-|LINE} {|line}"
  ]
};

var tubeSeverities = {
  specialService: 0,
  closed: 1,
  suspended: 2,
  partSuspended: 3,
  plannedClosure: 4,
  partClosure: 5,
  severeDelays: 6,
  reducedService: 7,
  busService: 8,
  minorDelays: 9,
  goodService: 10,
  partClosed: 11,
  exitOnly: 12,
  noStepFreeAccess: 13,
  changeOfFrequency: 14,
  diverted: 15,
  notRunning: 16,
  issuesReported: 17,
  noIssues: 18,
  information: 19,
  serviceClosed: 20
};

/**
 * Generates the text to respond to the specified line statuses response.
 * @param {Object[]} data - An array of line statuses.
 * @returns {String} The text response for the specified line statuses.
 */
intent.generateResponse = function (data) {

  if (data && data.length > 0) {

    var line = data[0];

    if (line.lineStatuses.length > 0) {

      var status = line.lineStatuses[0];

      switch (status.statusSeverity) {

        case tubeSeverities.goodService:
        case tubeSeverities.noIssues:
          return "There is a good service on the " + line.name + " line.";

        case tubeSeverities.closed:
        case tubeSeverities.serviceClosed:
          return "The " + line.name + " line is closed.";

        case tubeSeverities.minorDelays:
          return "There are minor delays on the " + line.name + " line.";

        case tubeSeverities.partClosed:
          return "The " + line.name + " line is partially closed.";

        case tubeSeverities.partSuspended:
          return "The " + line.name + " line is partially suspended.";

        case tubeSeverities.reducedService:
          return "There is a reduced service on the " + line.name + " line.";

        case tubeSeverities.severeDelays:
          return "There are severe delays on the " + line.name + " line.";

        case tubeSeverities.suspended:
          return "The " + line.name + " line is suspended.";

        default:
          return "There is currently disruption on the " + line.name + " line.";
      }
    }
  }

  return responses.onError;
};

/**
 * Maps the specified line name to a TfL API line Id.
 * @param {String} line - The line name.
 * @returns {String} The id for the specified line, if valid; otherwise null.
 */
intent.mapLineToId = function (line) {

  var normalized = (line || "").toLowerCase();

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

    case "dlr":
    case "docklands":
    case "docklands light railway":
    case "docklands railway":
      return "dlr";

    case "hammersmith and city":
    case "hammersmith & city":
      return "hammersmith-city";

    case "waterloo and city":
    case "waterloo & city":
      return "waterloo-city";

    default:
      return null;
  }
};

/**
 * Handles the intent for disruption.
 * @param {Object} request - The Alexa skill request.
 * @param {Object} response - The Alexa skill response.
 * @returns {Object} The result of the intent handler.
 */
intent.handler = function (request, response) {
  var line = intent.mapLineToId(request.slot("LINE"));
  if (line) {
    intent.api.getLineStatus(line)
      .then(function (data) {
        response
          .say(intent.generateResponse(data))
          .send();
      });
    return false;
  } else {
    response.say(responses.onUnknown);
  }
};

module.exports = intent;
