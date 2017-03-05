// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var api = require("../src/api.js");
var app = require("../index.js");
var assert = require("assert");
var helpers = require("./helpers.js");
var nock = require("nock");

describe("Integration", function () {

  beforeEach(function () {
    process.env.VERIFY_SKILL_ID = "true";
  });

  describe("When the skill is launched", function () {

    var actual;

    beforeEach(function (done) {

      var json = helpers.launchRequest();

      app.request(json).then(function (response) {
        actual = response;
        done();
      });
    });

    it("Then there is a response", function () {
      assert.notEqual(actual, null);
      assert.notEqual(actual.response, null);
    });
    it("Then the session does not end", function () {
      assert.equal(actual.response.shouldEndSession, false);
    });
    it("Then the speech is correct", function () {
      assert.notEqual(actual.response.outputSpeech, null);
      assert.equal(actual.response.outputSpeech.type, "SSML");
      assert.equal(actual.response.outputSpeech.ssml, "<speak>Welcome to London Travel. You can ask me about disruption or for the status of any tube line, London Overground or the D.L.R..</speak>");
    });
  });

  describe("When the disruption intent is requested", function () {

    var json;

    beforeEach(function () {

      api.appId = "MyApplicationId";
      api.appKey = "MyApplicationKey";

      json = helpers.intentRequest("DisruptionIntent");
    });

    describe("Given there is no disruption", function () {

      var actual;

      beforeEach(function (done) {

        nock("https://api.tfl.gov.uk")
          .get("/Line/Mode/dlr,overground,tube/Disruption")
          .query({ app_id: "MyApplicationId", app_key: "MyApplicationKey" })
          .reply(200, []);

        app.request(json).then(function (response) {
          actual = response;
          done();
        });
      });

      it("Then there is a response", function () {
        assert.notEqual(actual, null);
        assert.notEqual(actual.response, null);
      });
      it("Then the session ends", function () {
        assert.equal(actual.response.shouldEndSession, true);
      });
      it("Then the speech is correct", function () {
        assert.notEqual(actual.response.outputSpeech, null);
        assert.equal(actual.response.outputSpeech.type, "SSML");
        assert.equal(actual.response.outputSpeech.ssml, "<speak>There is currently no disruption on the tube, London Overground or the D.L.R..</speak>");
      });
      it("Then the card is correct", function () {
        assert.notEqual(actual.response.card, null);
        assert.equal(actual.response.card.type, "Standard");
        assert.equal(actual.response.card.title, "Disruption Summary");
        assert.equal(actual.response.card.text, "There is currently no disruption on the tube, London Overground or the DLR.");
      });
    });

    describe("Given there is disruption", function () {

      var actual;

      beforeEach(function (done) {

        nock("https://api.tfl.gov.uk")
          .get("/Line/Mode/dlr,overground,tube/Disruption")
          .query({ app_id: "MyApplicationId", app_key: "MyApplicationKey" })
          .reply(200, [
            {
              description: "DLR: Minor delays."
            },
            {
              description: "Waterloo & City line: Severe delays."
            },
            {
              description: "Waterloo & City line: Severe delays."
            }
          ]);

        app.request(json).then(function (response) {
          actual = response;
          done();
        });
      });

      it("Then there is a response", function () {
        assert.notEqual(actual, null);
        assert.notEqual(actual.response, null);
      });
      it("Then the session ends", function () {
        assert.equal(actual.response.shouldEndSession, true);
      });
      it("Then the speech is correct", function () {
        assert.notEqual(actual.response.outputSpeech, null);
        assert.equal(actual.response.outputSpeech.type, "SSML");
        assert.equal(actual.response.outputSpeech.ssml, "<speak><p>D.L.R.: Minor delays.</p> <p>Waterloo and City line: Severe delays.</p> <p>There is a good service on all other lines.</p></speak>");
      });
      it("Then the card is correct", function () {
        assert.notEqual(actual.response.card, null);
        assert.equal(actual.response.card.type, "Standard");
        assert.equal(actual.response.card.title, "Disruption Summary");
        assert.equal(actual.response.card.text, "DLR: Minor delays.\nWaterloo & City line: Severe delays.");
      });
    });

    describe("Given an error occurs", function () {

      var actual;

      beforeEach(function (done) {

        nock("https://api.tfl.gov.uk")
          .get("/Line/Mode/dlr,overground,tube/Disruption")
          .query({ app_id: "MyApplicationId", app_key: "MyApplicationKey" })
          .reply(500);

        app.request(json).then(function (response) {
          actual = response;
          done();
        });
      });

      it("Then there is a response", function () {
        assert.notEqual(actual, null);
        assert.notEqual(actual.response, null);
      });
      it("Then the session ends", function () {
        assert.equal(actual.response.shouldEndSession, true);
      });
      it("Then the speech is correct", function () {
        assert.notEqual(actual.response.outputSpeech, null);
        assert.equal(actual.response.outputSpeech.type, "SSML");
        assert.equal(actual.response.outputSpeech.ssml, "<speak>Sorry, something went wrong.</speak>");
      });
    });
  });

  describe("When the status intent is requested", function () {

    var json;

    beforeEach(function () {

      api.appId = "MyApplicationId";
      api.appKey = "MyApplicationKey";

      json = helpers.intentRequest("StatusIntent", {
        "LINE": {
          "value": "waterloo & city",
          "name": "LINE"
        }
      });
    });

    describe("Given there is a good service", function () {

      var actual;

      beforeEach(function (done) {

        nock("https://api.tfl.gov.uk")
          .get("/Line/waterloo-city/Status")
          .query({ app_id: "MyApplicationId", app_key: "MyApplicationKey" })
          .reply(200, [
            {
              "id": "waterloo-city",
              "name": "Waterloo & City",
              "lineStatuses": [
                {
                  "statusSeverity": 10
                }
              ]
            }
          ]);

        app.request(json).then(function (response) {
          actual = response;
          done();
        });
      });

      it("Then there is a response", function () {
        assert.notEqual(actual, null);
        assert.notEqual(actual.response, null);
      });
      it("Then the session ends", function () {
        assert.equal(actual.response.shouldEndSession, true);
      });
      it("Then the speech is correct", function () {
        assert.notEqual(actual.response.outputSpeech, null);
        assert.equal(actual.response.outputSpeech.type, "SSML");
        assert.equal(actual.response.outputSpeech.ssml, "<speak>There is a good service on the Waterloo and City line.</speak>");
      });
      it("Then the card is correct", function () {
        assert.notEqual(actual.response.card, null);
        assert.equal(actual.response.card.type, "Standard");
        assert.equal(actual.response.card.title, "Waterloo & City Line Status");
        assert.equal(actual.response.card.text, "There is a good service on the Waterloo and City line.");
      });
    });

    describe("Given there is disruption", function () {

      var actual;

      beforeEach(function (done) {

        nock("https://api.tfl.gov.uk")
          .get("/Line/waterloo-city/Status")
          .query({ app_id: "MyApplicationId", app_key: "MyApplicationKey" })
          .reply(200, [
            {
              "id": "waterloo-city",
              "name": "Waterloo & City",
              "lineStatuses": [
                {
                  "statusSeverity": 6,
                  "reason": "Waterloo & City Line: SEVERE DELAYS due to a person ill on a train earlier at Waterloo.",
                  "disruption": {
                    "description": "Waterloo & City Line: SEVERE DELAYS due to a person ill on a train earlier at Waterloo."
                  }
                }
              ]
            }
          ]);

        app.request(json).then(function (response) {
          actual = response;
          done();
        });
      });

      it("Then there is a response", function () {
        assert.notEqual(actual, null);
        assert.notEqual(actual.response, null);
      });
      it("Then the session ends", function () {
        assert.equal(actual.response.shouldEndSession, true);
      });
      it("Then the speech is correct", function () {
        assert.notEqual(actual.response.outputSpeech, null);
        assert.equal(actual.response.outputSpeech.type, "SSML");
        assert.equal(actual.response.outputSpeech.ssml, "<speak>SEVERE DELAYS due to a person ill on a train earlier at Waterloo.</speak>");
      });
      it("Then the card is correct", function () {
        assert.notEqual(actual.response.card, null);
        assert.equal(actual.response.card.type, "Standard");
        assert.equal(actual.response.card.title, "Waterloo & City Line Status");
        assert.equal(actual.response.card.text, "SEVERE DELAYS due to a person ill on a train earlier at Waterloo.");
      });
    });

    describe("Given an error occurs", function () {

      var actual;

      beforeEach(function (done) {

        nock("https://api.tfl.gov.uk")
          .get("/Line/waterloo-city/Status")
          .query({ app_id: "MyApplicationId", app_key: "MyApplicationKey" })
          .reply(500);

        app.request(json).then(function (response) {
          actual = response;
          done();
        });
      });

      it("Then there is a response", function () {
        assert.notEqual(actual, null);
        assert.notEqual(actual.response, null);
      });
      it("Then the session ends", function () {
        assert.equal(actual.response.shouldEndSession, true);
      });
      it("Then the speech is correct", function () {
        assert.notEqual(actual.response.outputSpeech, null);
        assert.equal(actual.response.outputSpeech.type, "SSML");
        assert.equal(actual.response.outputSpeech.ssml, "<speak>Sorry, something went wrong.</speak>");
      });
    });
  });

  describe("When help is requested", function () {

    var actual;

    beforeEach(function (done) {

      var json = helpers.intentRequest("AMAZON.HelpIntent");

      app.request(json).then(function (response) {
        actual = response;
        done();
      });
    });

    it("Then there is a response", function () {
      assert.notEqual(actual, null);
      assert.notEqual(actual.response, null);
    });
    it("Then the session does not end", function () {
      assert.equal(actual.response.shouldEndSession, false);
    });
    it("Then the speech is correct", function () {
      assert.notEqual(actual.response.outputSpeech, null);
      assert.equal(actual.response.outputSpeech.type, "SSML");
      assert.equal(actual.response.outputSpeech.ssml, "<speak><p>This skill allows you to check for the status of a specific line, or for disruption in general. You can ask about any London Underground line, London Overground or the Docklands Light Railway.</p> <p>Asking about disruption in general provides information about any lines that are currently experiencing issues, such as any delays or planned closures.</p> <p>Asking for the status for a specific line provides a summary of the current service, such as whether there is a good service or if there are any delays.</p></speak>");
    });
  });

  describe("When cancellation is requested", function () {

    var actual;

    beforeEach(function (done) {

      var json = helpers.intentRequest("AMAZON.CancelIntent");

      app.request(json).then(function (response) {
        actual = response;
        done();
      });
    });

    it("Then there is a response", function () {
      assert.notEqual(actual, null);
      assert.notEqual(actual.response, null);
    });
    it("Then the session ends", function () {
      assert.equal(actual.response.shouldEndSession, true);
    });
    it("Then there is no speech response", function () {
      assert.equal(actual.response.outputSpeech, null);
    });
  });

  describe("When stop is requested", function () {

    var actual;

    beforeEach(function (done) {

      var json = helpers.intentRequest("AMAZON.StopIntent");

      app.request(json).then(function (response) {
        actual = response;
        done();
      });
    });

    it("Then there is a response", function () {
      assert.notEqual(actual, null);
      assert.notEqual(actual.response, null);
    });
    it("Then the session ends", function () {
      assert.equal(actual.response.shouldEndSession, true);
    });
    it("Then there is no speech response", function () {
      assert.equal(actual.response.outputSpeech, null);
    });
  });

  describe("When the session is ended", function () {

    var actual;

    beforeEach(function (done) {

      var json = helpers.sessionEndRequest();

      app.request(json).then(function (response) {
        actual = response;
        done();
      });
    });

    it("Then there is a response", function () {
      assert.notEqual(actual, null);
      assert.notEqual(actual.response, null);
    });
    it("Then the session ends", function () {
      assert.equal(actual.response.shouldEndSession, true);
    });
    it("Then the speech is correct", function () {
      assert.notEqual(actual.response.outputSpeech, null);
      assert.equal(actual.response.outputSpeech.type, "SSML");
      assert.equal(actual.response.outputSpeech.ssml, "<speak>Goodbye.</speak>");
    });
  });

  describe("When the application Id is incorrect.", function () {

    var actual;

    beforeEach(function (done) {

      var json = helpers.sessionEndRequest();

      json.context.System.application.applicationId = "not my application id";
      json.session.application.applicationId = "not my application id";

      app.request(json).catch(function (error) {
        actual = error;
        done();
      });
    });

    it("Then the request fails", function () {
      assert.equal(actual, "Invalid application Id.");
    });
  });
});
