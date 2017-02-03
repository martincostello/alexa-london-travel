// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var api = require("./../api");
var responses = require("./../responses");
var sprintf = require("sprintf");

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

    if (line.lineStatuses && line.lineStatuses.length > 0) {

      var status = line.lineStatuses[0];

      if (typeof status.statusSeverity === "number") {

        var format;

        switch (status.statusSeverity) {

          case tubeSeverities.goodService:
          case tubeSeverities.noIssues:
            format = "There is a good service on the %s%s.";
            break;

          case tubeSeverities.busService:
            format = "Some parts of the %s%s are currently being served by a replacement bus service.";
            break;

          case tubeSeverities.closed:
          case tubeSeverities.notRunning:
          case tubeSeverities.serviceClosed:
            format = "The %s%s is closed.";
            break;

          case tubeSeverities.minorDelays:
            format = "There are minor delays on the %s%s.";
            break;

          case tubeSeverities.partClosed:
          case tubeSeverities.partClosure:
            format = "The %s%s is partially closed.";
            break;

          case tubeSeverities.partSuspended:
            format = "The %s%s is partially suspended.";
            break;

          case tubeSeverities.plannedClosure:
            format = "There is a planned closure on the %s%s.";
            break;

          case tubeSeverities.reducedService:
            format = "There is a reduced service on the %s%s.";
            break;

          case tubeSeverities.severeDelays:
            format = "There are severe delays on the %s%s.";
            break;

          case tubeSeverities.suspended:
            format = "The %s%s is suspended.";
            break;

          default:
            format = "There is currently disruption on the %s%s.";
            break;
        }

        var isDLR = line.name.toLowerCase() === "dlr";

        var spokenName;
        var suffix;

        if (isDLR === true) {
          spokenName = "D.L.R.";
          suffix = "";
        } else {
          spokenName = line.name;
          suffix = " line";
        }

        return sprintf(format, spokenName, suffix);
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
    case "northern":
    case "piccadilly":
    case "victoria":
      return normalized;

    case "met":
    case "metropolitan":
      return "metropolitan";

    case "dlr":
    case "docklands":
    case "docklands light railway":
    case "docklands railway":
      return "dlr";

    case "hammersmith":
    case "hammersmith and city":
    case "hammersmith & city":
      return "hammersmith-city";

    case "city":
    case "waterloo":
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
