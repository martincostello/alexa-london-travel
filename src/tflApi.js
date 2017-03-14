// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var constants = require("./constants");
var http = require("request-promise");

const TflApiHost = "https://api.tfl.gov.uk/";

var tflApi = {
  appId: process.env.TFL_APP_ID || "",
  appKey: process.env.TFL_APP_KEY || ""
};

tflApi.httpGet = function (path) {
  var options = {
    uri: TflApiHost + path,
    qs: {
      app_id: tflApi.appId,
      app_key: tflApi.appKey
    },
    headers: {
      "User-Agent": constants.appName + "/" + constants.version
    },
    json: true
  };
  return http(options);
};

tflApi.getDisruption = function (modes) {
  return tflApi.httpGet("Line/Mode/" + modes.concat() + "/Disruption");
};

tflApi.getLineStatus = function (id) {
  return tflApi.httpGet("Line/" + id + "/Status");
};

module.exports = tflApi;
