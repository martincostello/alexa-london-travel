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
 * Returns whether the specified line name refers to the Docklands Light Railway.
 * @param {String} name - The name of the line as reported from the TfL API.
 * @returns {Boolean} Whether the line is the DLR.
 */
intent.isDLR = function (name) {
  return name.toLowerCase() === "dlr";
};

/**
 * Returns whether the specified line name refers to the London Overground.
 * @param {String} name - The name of the line as reported from the TfL API.
 * @returns {Boolean} Whether the line is the London Overground.
 */
intent.isOverground = function (name) {
  return name.toLowerCase().indexOf("overground") > -1;
};

/**
 * @param {String} name - The name of the line as reported from the TfL API.
 * @returns {String} The spoken name of the line.
 */
intent.toSpokenLineName = function (name) {

  var isDLR = intent.isDLR(name);
  var isOverground = intent.isOverground(name);

  var prefix = "";
  var suffix = "";

  var spokenName;

  if (isDLR === true) {
    prefix = "the ";
    spokenName = "D.L.R.";
  } else if (isOverground === true) {
    spokenName = name;
    suffix = "";
  } else {
    prefix = "the ";
    spokenName = name;
    suffix = " line";
  }

  return sprintf("%s%s%s", prefix, spokenName, suffix);
};

/**
 * Returns the title to use for a card for the specified line name.
 * @param {String} name - The name of the line as reported from the TfL API.
 * @returns {String} The title to use for the card.
 */
intent.toCardTitle = function (name) {

  var isDLR = intent.isDLR(name);
  var isOverground = intent.isOverground(name);

  var suffix;
  var spokenName;

  if (isDLR === true || isOverground === true) {
    suffix = "";
  } else {
    suffix = " Line";
  }

  return sprintf("%s%s Status", name, suffix);
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

  if (text) {
    return responses.toSsml(text);
  }

  // Fallback response
  var spokenName = intent.toSpokenLineName(name);
  return responses.toSsml(sprintf("There are %d disruptions on the %s.", statuses.length, spokenName));
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

  if (typeof status.statusSeverity === "number") {

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

    var spokenName = intent.toSpokenLineName(name);
    var statusText = sprintf(format, spokenName);

    return statusText.charAt(0).toUpperCase() + statusText.slice(1);
  }

  return "";
};

/**
 * Generates the card to respond to the specified disruption text.
 * @param {String} line - The name of the line.
 * @param {String} text - The SSML response.
 * @returns {Object} The card object to use.
 */
intent.generateCard = function (line, text) {
  return {
    type: "Standard",
    title: intent.toCardTitle(line),
    text: text.replace("D.L.R.", "DLR")
  };
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

    case "london overground":
    case "overground":
      return "london-overground";

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
        var text = intent.generateResponse(data);
        var card = intent.generateCard(data[0].name, text);
        response
          .say(text)
          .card(card)
          .send();
      })
      .catch(function (err) {
        console.error("Failed to get line status: ", line, err);
        response.say(responses.onError);
      });
    return false;
  } else {
    response
      .say(responses.onUnknownLine)
      .reprompt(responses.toSsml(responses.onUnknownLine));
  }
};

module.exports = intent;
