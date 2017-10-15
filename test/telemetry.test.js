// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var assert = require("assert");
var simple = require("simple-mock");
var sinon = require("sinon");
var telemetry = require("../src/telemetry");

describe("Telemetry", function () {

  describe("When application insights is not configured", function () {

    var response;

    beforeEach(function () {

      sinon.spy(telemetry, "createClient");
      sinon.spy(telemetry.appInsights, "setup");
      sinon.spy(telemetry.appInsights, "start");

      telemetry.setup(null);
    });

    afterEach(function () {
      telemetry.createClient.restore();
      telemetry.appInsights.setup.restore();
      telemetry.appInsights.start.restore();
    });

    it("Then tracking an event does nothing", function () {
      telemetry.trackEvent("MyEvent", { foo: "bar" });
      assert.equal(telemetry.createClient.notCalled, true);
    });

    it("Then tracking an exception does nothing", function () {
      telemetry.trackException(new Error("My error"), { foo: "bar" });
      assert.equal(telemetry.createClient.notCalled, true);
    });
  });

  describe("When application insights is configured", function () {

    var instrumentationKey;
    var config;

    beforeEach(function () {

      config = {
      };

      config.setAutoCollectConsole = function () {
        return config;
      };

      config.setAutoCollectDependencies = function () {
        return config;
      };

      config.setAutoCollectExceptions = function () {
        return config;
      };

      config.setAutoCollectPerformance = function () {
        return config;
      };

      config.setAutoCollectRequests = function () {
        return config;
      };

      config.setAutoDependencyCorrelation = function () {
        return config;
      };

      config.setUseDiskRetryCaching = function () {
        return config;
      };

      config.start = function () {
        return config;
      };

      sinon.spy(config, "start");

      sinon.stub(telemetry.appInsights, "setup").returns(config);

      instrumentationKey = "my key";

      telemetry.setup(instrumentationKey);
    });

    afterEach(function () {
      telemetry.appInsights.setup.restore();
    });

    it("Then tracking is set up", function () {
      assert.equal(telemetry.appInsights.setup.calledWith(instrumentationKey), true);
    });

    it("Then tracking is started", function () {
      assert.equal(config.start.calledOnce, true);
    });

    it("Then a client can be created", function () {
      var client = telemetry.createClient();
      assert.notEqual(telemetry.appInsights.start.calledOnce, client);
    });

    describe("When telemetry is tracked", function () {

      var client;

      beforeEach(function () {

        client = {
          config: {
          },
          trackEvent: function () {
          },
          trackException: function () {
          }
        };

        sinon.spy(client, "trackEvent");
        sinon.spy(client, "trackException");

        sinon.stub(telemetry, "createClient").returns(client);
      });

      afterEach(function () {
        telemetry.createClient.restore();
      });

      it("Then an event is tracked", function () {

        var name = "My event";
        var properties = { foo: "bar" };

        telemetry.trackEvent(name, properties);

        assert.equal(client.trackEvent.calledWith({ name, properties }), true);
      });

      it("Then an exception is tracked", function () {

        var exception = new Error("My error");
        var properties = { foo: "bar" };

        telemetry.trackException(exception, properties);

        assert.equal(client.trackException.calledWith({ exception, properties }), true);
      });
    });
  });
});
