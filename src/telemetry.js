// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var appInsights = require("applicationinsights");

const telemetry = {
  appInsights: appInsights,
  trackEvent: function () {
  },
  trackException: function () {
  }
};

/**
 * Sets up collection of telemetry.
 * @param {String} [instrumentationKey=null] - The optional instrumentation key to use.
 */
telemetry.setup = function (instrumentationKey) {
  if (instrumentationKey) {

    telemetry.appInsights.setup(instrumentationKey).start();

    telemetry.trackEvent = function (name, properties) {
      var client = telemetry.appInsights.getClient();
      client.trackEvent(name, properties);
    };

    telemetry.trackException = function (exception, properties) {
      var client = telemetry.appInsights.getClient();
      client.trackException(exception, properties);
    };
  }
};

module.exports = telemetry;
