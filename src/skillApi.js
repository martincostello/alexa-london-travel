// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var constants = require("./constants");
var http = require("request-promise");
var errors = require('request-promise/errors');

var skillApi = {
  hostname: process.env.SKILL_API_HOSTNAME || "https://londontravel.martincostello.com/"
};

skillApi.httpGet = function (path, accessToken) {
  var options = {
    uri: skillApi.hostname + path,
    headers: {
      "Authorization": "Bearer " + accessToken,
      "User-Agent": constants.appName + "/" + constants.version
    },
    json: true
  };
  return http(options);
};

/**
 * Gets the preferences for the user associated with the specified access token.
 * @param {String} accessToken - The user's access token.
 * @returns {Object} - The preferences for the specified user if the access token is valid; otherwise null.
 */
skillApi.getPreferences = function (accessToken) {
  return skillApi
    .httpGet("api/preferences", accessToken)
    .catch(errors.StatusCodeError, function (reason) {
      if (reason.statusCode === 401) {
        return null;
      }
      else {
        throw new Error("Failed to get user preferences using the London Travel Skill API: " + reason.statusCode);
      }
    });
};

module.exports = skillApi;
