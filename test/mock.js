// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var nock = require("nock");

var mock = {

};

/**
 * Sets up a successful mock response for the TfL API.
 * @param {String} path - The path to setup the response for.
 * @param {Object} [response=""] - The optional response content to return.
 */
mock.success = function (path, response) {
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
mock.failure = function (path, response) {
  nock("https://api.tfl.gov.uk")
    .get(path)
    .query({ app_id: "MyApplicationId", app_key: "MyApplicationKey" })
    .reply(500, response || "");
};

/**
 * Sets up a mock response for the TfL API.
 * @param {String} path - The path to setup the response for.
 * @param {Number} statusCode - The HTTP status code to return.
 * @param {Object} [response=""] - The optional response content to return.
 */
mock.setupApi = function (path, statusCode, response) {
  nock("https://api.tfl.gov.uk")
    .get(path)
    .query({ app_id: "MyApplicationId", app_key: "MyApplicationKey" })
    .reply(statusCode, response || "");
};

module.exports = mock;
