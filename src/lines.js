// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var sprintf = require("sprintf");
var verbalizer = require("./verbalizer");

var lines = {
};

/**
 * Returns whether the specified line name refers to the Docklands Light Railway.
 * @param {String} name - The name of the line as reported from the TfL API.
 * @returns {Boolean} Whether the line is the DLR.
 */
lines.isDLR = function (name) {
  return name.toLowerCase() === "dlr";
};

/**
 * Returns whether the specified line name refers to the London Overground.
 * @param {String} name - The name of the line as reported from the TfL API.
 * @returns {Boolean} Whether the line is the London Overground.
 */
lines.isOverground = function (name) {
  return name.toLowerCase().indexOf("overground") > -1;
};

/**
 * Returns the spoken version of the specified line name.
 * @param {String} name - The name of the line as reported from the TfL API.
 * @param {Boolean} [asTitle=false] - Whether to format as a title.
 * @returns {String} The spoken name of the line.
 */
lines.toSpokenName = function (name, asTitle) {

  var prefix = "";
  var suffix = "";

  var spokenName;

  if (lines.isDLR(name) === true) {
    prefix = "the ";
    spokenName = verbalizer.verbalize("DLR");
  } else if (lines.isOverground(name) === true) {
    spokenName = name;
  } else {
    prefix = "the ";
    spokenName = name;
    suffix = asTitle ? " Line" : " line";
  }

  if (asTitle === true) {
    return sprintf("%s%s", spokenName, suffix);
  }
  else {
    return sprintf("%s%s%s", prefix, spokenName, suffix);
  }
};

module.exports = lines;
