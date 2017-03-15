// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var nock = require("nock");

var mock = {
  skill: {
    preferences: function (accessToken) {
      return nock("https://londontravel.martincostello.com")
        .matchHeader("authorization", "Bearer " + accessToken)
        .matchHeader("user-agent", /alexa-london-travel\/.*/)
        .get("/api/preferences");
    }
  },
  tfl: {
  }
};

/**
 * Sets up a successful mock response for the skill API.
 * @param {String} accessToken - The access token to use.
 * @param {Object} response - The response content to return.
 */
mock.skill.success = function (accessToken, response) {
   mock.skill.preferences(accessToken)
     .reply(200, response);
};

/**
 * Sets up an unauthorized mock response for the skill API.
 * @param {String} accessToken - The access token to use.
 */
mock.skill.unauthorized = function (accessToken) {
   mock.skill.preferences(accessToken)
     .reply(401, {});
};

/**
 * Sets up a failed mock response for the skill API.
 * @param {String} accessToken - The access token to use.
 */
mock.skill.failure = function (accessToken) {
   mock.skill.preferences(accessToken)
     .reply(500, {});
};

/**
 * Sets up a successful mock response for the TfL API.
 * @param {String} path - The path to setup the response for.
 * @param {Object} [response=""] - The optional response content to return.
 */
mock.tfl.success = function (path, response) {
  nock("https://api.tfl.gov.uk")
    .get(path)
    .query({ app_id: "MyApplicationId", app_key: "MyApplicationKey" })
    .reply(200, response || "");
};

/**
 * Sets up a failed mock response for the TfL API.
 * @param {String} path - The path to setup the response for.
 * @param {Object} [response=""] - The optional response content to return.
 */
mock.tfl.failure = function (path, response) {
  nock("https://api.tfl.gov.uk")
    .get(path)
    .query({ app_id: "MyApplicationId", app_key: "MyApplicationKey" })
    .reply(500, response || "");
};

module.exports = mock;
