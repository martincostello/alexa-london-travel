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

telemetry.instrumentationKey = "";

telemetry.createClient = function () {
  var client = new appInsights.TelemetryClient(telemetry.instrumentationKey);

  // Prevent Lambda function from hanging and timing out.
  // See https://github.com/martincostello/alexa-london-travel/issues/45.
  client.config.maxBatchIntervalMs = 0;
  client.config.maxBatchSize = 1;

  return client;
};

/**
 * Sets up collection of telemetry.
 * @param {String} [instrumentationKey=null] - The optional instrumentation key to use.
 */
telemetry.setup = function (instrumentationKey) {
  if (instrumentationKey) {

    telemetry.instrumentationKey = instrumentationKey;

    telemetry.appInsights.setup(instrumentationKey).start();

    telemetry.trackEvent = function (name, properties) {
      telemetry.createClient().trackEvent({
        name: name,
        properties: properties
      });
    };

    telemetry.trackException = function (exception, properties) {
      telemetry.createClient().trackException({
        exception: exception,
        properties: properties
      });
    };
  }
};

module.exports = telemetry;
