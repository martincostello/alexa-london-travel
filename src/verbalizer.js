// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var verbalizer = {
};

/**
 * Returns a string which better represents the specified text when spoken.
 * @param {String} text - The text to verbalize.
 * @returns {String} The representation of the text that is enhanced for being spoken aloud by Alexa.
 */
verbalizer.verbalize = function (text) {
  return text
    .replace("DLR", "D.L.R.")
    .replace("e/b", "eastbound")
    .replace("n/b", "northbound")
    .replace("s/b", "southbound")
    .replace("w/b", "westbound")
    .replace("SWT", "South West Trains");
};

module.exports = verbalizer;
