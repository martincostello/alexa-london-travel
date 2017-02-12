// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var intent = {
  name: "AMAZON.StopIntent",
  enabled: true
};

/**
 * Handles the intent for stop.
 * @param {Object} request - The Alexa skill request.
 * @param {Object} response - The Alexa skill response.
 */
intent.handler = function (request, response) {
  response.shouldEndSession(true);
};

module.exports = intent;
