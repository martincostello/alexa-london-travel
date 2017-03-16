// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var lines = require("./lines");
var responses = require("./responses");
var sprintf = require("sprintf");

var cards = {
  commuteIntentCardTitle: "Your Commute"
};

/**
 * Generates a standard card.
 * @param {String} title - The title of the card.
 * @param {String} text - The text of the card.
 * @returns {Object} The card object to use.
 */
cards.standard = function (title, text) {
  return {
    type: "Standard",
    title: title,
    text: cards.normalizeText(text)
  };
};

/**
 * Generates the card for a commute from the specified statuses.
 * @param {String[]} statuses - An array of statuses.
 * @returns {Object} The card object to use.
 */
cards.forCommute = function (statuses) {

  var card = cards.forDisruption(statuses);

  card.title = cards.commuteIntentCardTitle;

  return card;
};

/**
 * Generates the card to respond to the specified disruption status(es).
 * @param {String[]} statuses - An array of disruption descriptions.
 * @returns {Object} The card object to use.
 */
cards.forDisruption = function (statuses) {

  var text;

  if (statuses.length === 0) {
    text = responses.onNoDisruption;
  } else {
    text = statuses.join("\n");
  }

  return cards.standard(
    "Disruption Summary",
    cards.normalizeText(text)
  );
};

/**
 * Generates the card to respond to the specified status text.
 * @param {String} line - The name of the line.
 * @param {String} text - The SSML response.
 * @returns {Object} The card object to use.
 */
cards.forStatus = function (line, text) {
  return cards.standard(
    cards.toStatusCardTitle(line),
    text
  );
};

/**
 * Normalizes the specified text for use in a card.
 * @param {String} text - The text to normalize.
 * @returns {Object} The normalized text for a card object to use.
 */
cards.normalizeText = function (text) {
  return text.replace("D.L.R.", "DLR");
};

/**
 * Returns the title to use for a status card for the specified line name.
 * @param {String} name - The name of the line as reported from the TfL API.
 * @returns {String} The title to use for the status card.
 */
cards.toStatusCardTitle = function (name) {

  var suffix;

  if (lines.isDLR(name) === true || lines.isOverground(name) === true) {
    suffix = "";
  } else {
    suffix = " Line";
  }

  return sprintf("%s%s Status", name, suffix);
};

module.exports = cards;
