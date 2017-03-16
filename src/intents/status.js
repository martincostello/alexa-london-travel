// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var cards = require("./../cards");
var lines = require("./../lines");
var responses = require("./../responses");
var sprintf = require("sprintf");
var tflApi = require("./../tflApi");

var intent = {
  api: tflApi,
  name: "StatusIntent",
  enabled: true,
  slots: {
    "LINE": "LINE_NAME"
  },
  utterances: [
    "about the {|status of the} {-|LINE} {|line} {|now|right now|this morning|this afternoon|this evening|today|tonight}",
    "for the status of the {-|LINE} {|line} {|now|right now|this morning|this afternoon|this evening|today|tonight}",
    "how the {-|LINE} is {|doing} {|now|right now|this morning|this afternoon|this evening|today|tonight}",
    "{|what|what is|what's} {|the} status of the {-|LINE} {|line} {|is} {|now|right now|this morning|this afternoon|this evening|today|tonight}"
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
    var text;

    if (line.lineStatuses && line.lineStatuses.length > 0) {

      if (line.lineStatuses.length === 1) {

        var lineStatus = line.lineStatuses[0];

        var includeDetail = intent.shouldStatusUseCustomResponse(lineStatus.statusSeverity) === false;

        text = intent.generateResponseForSingleStatus(
          line.name,
          lineStatus,
          includeDetail);

        if (text) {
          return responses.toSsml(text);
        }
      } else {
        return intent.generateResponseForMultipleStatuses(line.name, line.lineStatuses);
      }
    }
  }

  return responses.onError;
};

/**
 * Generates the text to respond for a multiple line statuses.
 * @param {String} name - The name of the line.
 * @param {Object} status - The statuses for the line.
 * @returns {String} The text response for the specified line statuses.
 */
intent.generateResponseForMultipleStatuses = function (name, statuses) {

  // The descriptions appear to reference each other, so use the least severe's
  var sorted = statuses.sort(function (a, b) {
    if (a.statusSeverity > b.statusSeverity) {
      return -1;
    } else if (a.statusSeverity < b.statusSeverity) {
      return 1;
    } else {
      return 0;
    }
  });

  var text = intent.generateResponseForSingleStatus(
    name,
    sorted[0],
    true);

  return responses.toSsml(text);
};

/**
 * Generates the text to respond for a single line status.
 * @param {String} name - The name of the line.
 * @param {Object} status - The status for a line.
 * @param {Boolean} includeDetail = Whether to include the detail in the response.
 * @returns {String} The text response for the specified line status.
 */
intent.generateResponseForSingleStatus = function (name, status, includeDetail) {
  if (includeDetail !== true) {
    return intent.generateSummaryResponse(name, status);
  } else {
    return intent.generateDetailedResponse(status);
  }
};

/**
 * Generates the detailed text to respond for a single line status.
 * @param {Object} status - The status for a line.
 * @returns {String} The text response for the specified line status.
 */
intent.generateDetailedResponse = function (status) {

  var response = "";

  if (status.reason) {

    response = status.reason;

    // Trim off the line name prefix, if present
    var delimiter = ": ";
    var index = response.indexOf(delimiter);

    if (index > -1) {
      response = response.slice(index + delimiter.length);
    }
  }

  return response;
};

/**
 * Returns whether the specified status severity should use a custom response.
 * @param {Number} statusSeverity - The status severity value.
 * @returns {Boolean} true if the status should use a custom response; otherwise false.
 */
intent.shouldStatusUseCustomResponse = function (statusSeverity) {
  switch (statusSeverity) {

    case tubeSeverities.goodService:
    case tubeSeverities.noIssues:
    case tubeSeverities.serviceClosed:
      return true;

    default:
      return false;
  }
};

/**
 * Generates the summary text to respond for a single line status.
 * @param {String} name - The name of the line.
 * @param {Object} status - The status for a line.
 * @returns {String} The text response for the specified line status.
 */
intent.generateSummaryResponse = function (name, status) {

  var format;

  switch (status.statusSeverity) {

    case tubeSeverities.goodService:
    case tubeSeverities.noIssues:
      format = "There is a good service on %s.";
      break;

    case tubeSeverities.busService:
      format = "Some parts of %s are currently being served by a replacement bus service.";
      break;

    case tubeSeverities.closed:
    case tubeSeverities.notRunning:
    case tubeSeverities.serviceClosed:
      format = "%s is closed.";
      break;

    case tubeSeverities.minorDelays:
      format = "There are minor delays on %s.";
      break;

    case tubeSeverities.partClosed:
    case tubeSeverities.partClosure:
      format = "%s is partially closed.";
      break;

    case tubeSeverities.partSuspended:
      format = "%s is partially suspended.";
      break;

    case tubeSeverities.plannedClosure:
      format = "There is a planned closure on %s.";
      break;

    case tubeSeverities.reducedService:
      format = "There is a reduced service on %s.";
      break;

    case tubeSeverities.severeDelays:
      format = "There are severe delays on %s.";
      break;

    case tubeSeverities.suspended:
      format = "%s is suspended.";
      break;

    default:
      format = "There is currently disruption on %s.";
      break;
  }

  var spokenName = lines.toSpokenName(name);
  var statusText = sprintf(format, spokenName);

  return statusText.charAt(0).toUpperCase() + statusText.slice(1);
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
    case "dlr":
    case "hammersmith-city":
    case "jubilee":
    case "london-overground":
    case "metropolitan":
    case "northern":
    case "piccadilly":
    case "victoria":
    case "waterloo-city":
      return normalized;

    case "london overground":
    case "overground":
      return "london-overground";

    case "met":
      return "metropolitan";

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
 * Gets the name and status of the specified line.
 * @param {String} line - The Id of the line to get the status for.
 * @returns {Object} A promise that returns an object containing the name of the line and the status text as SSML.
 */
intent.getLineStatus = function (line) {
  return intent.api.getLineStatus(line)
    .then(function (data) {
      var rawName = data[0].name;
      return {
        name: rawName,
        text: intent.generateResponse(data)
      };
    });
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
    return intent.getLineStatus(line)
      .then(function (result) {
        var text = result.text;
        var card = cards.forStatus(result.name, text);
        response
          .say(text)
          .card(card);
      })
      .catch(function (err) {
        console.error("Failed to get line status:", line, err);
        response.say(responses.onError);
      });
  } else {
    var text = responses.toSsml(responses.onUnknownLine);
    response
      .say(text)
      .reprompt(text);
  }
};

module.exports = intent;
