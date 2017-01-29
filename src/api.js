// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var constants = require("./constants");
var http = require("request-promise");

const ApiHost = "https://api.tfl.gov.uk/";

var httpGet = function (path) {
  var options = {
    uri: ApiHost + path,
    qs: {
      app_id: process.env.TFL_APP_ID || "",
      app_key: process.env.TFL_APP_KEY || ""
    },
    headers: {
      "User-Agent": constants.appName + "/" + constants.version
    },
    json: true
  };
  return http(options);
};

var api = {
  getDisruption: function (modes) {
    return httpGet("Line/Mode/" + modes.concat() + "/Disruption");
  },
  getLineStatus: function (id) {
    return httpGet("Line/" + id + "/Status");
  }
};

module.exports = api;
