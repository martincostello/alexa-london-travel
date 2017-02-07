// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var constants = require("./constants");
var http = require("request-promise");

const ApiHost = "https://api.tfl.gov.uk/";

var api = {
  appId: process.env.TFL_APP_ID || "",
  appKey: process.env.TFL_APP_KEY || ""
};

api.httpGet = function (path) {
  var options = {
    uri: ApiHost + path,
    qs: {
      app_id: api.appId,
      app_key: api.appKey
    },
    headers: {
      "User-Agent": constants.appName + "/" + constants.version
    },
    json: true
  };
  return http(options);
};

api.getDisruption = function (modes) {
  return api.httpGet("Line/Mode/" + modes.concat() + "/Disruption");
};

api.getLineStatus = function (id) {
  return api.httpGet("Line/" + id + "/Status");
};

module.exports = api;
