// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var assert = require("assert");
var dataDriven = require("data-driven");
var intent = require("../../src/intents/disruption");
var simple = require("simple-mock");

describe("Disruption Intent", function () {

  describe("When disruptions is null", function () {

    var data;
    var actual;

    beforeEach(function () {
      data = null;
      actual = null;
    });

    it("Then the response is that there are no disruptions", function () {
      actual = intent.generateResponse(data);
      assert.equal(actual, "There is currently no disruption on the tube, London Overground or the D.L.R..");
    });
  });

  describe("When there are no disruptions", function () {

    var data;
    var actual;

    beforeEach(function () {
      data = [];
      actual = null;
    });

    it("Then the response is that there are no disruptions", function () {
      actual = intent.generateResponse(data);
      assert.equal(actual, "There is currently no disruption on the tube, London Overground or the D.L.R..");
    });
  });

  describe("When there is one disruption", function () {

    var testCases = [
      { description: "There are severe delays on the District Line.", expected: "There are severe delays on the District Line." },
      { description: "There are minor delays on the Hammersmith & City Line.", expected: "There are minor delays on the Hammersmith & City Line." }
    ];

    dataDriven(testCases, function () {
      it("Then the response is the description of the single disruption", function (context) {

        var data = [
          { description: context.description }
        ];

        var actual = intent.generateRawResponse(data);
        assert.deepEqual(actual, [context.expected]);
      });
    });
  });

  describe("When there are multiple disruptions", function () {

    var data;
    var actual;

    beforeEach(function () {

      data = [
        { description: "There are severe delays on the District Line." },
        { description: "There are severe delays on the District Line." },
        { description: "There are minor delays on the Circle Line." }
      ];

      actual = null;
    });

    it("Then the response is the description of the first disruption", function () {
      actual = intent.generateRawResponse(data);
      assert.deepEqual(actual, ["There are severe delays on the District Line.", "There are minor delays on the Circle Line."]);
    });
  });

  describe("When generating a card", function () {

    var actual;

    beforeEach(function () {
      var statuses = [];
      actual = intent.generateCard(statuses);
    });

    it("Then a card is returned", function () {
      assert.notEqual(actual, null);
    });
    it("Then the type is correct", function () {
      assert.equal(actual.type, "Standard");
    });
    it("Then the title is correct", function () {
      assert.equal(actual.title, "Disruption Summary");
    });
    it("Then the text is correct", function () {
      assert.equal(actual.text, "There is currently no disruption on the tube, London Overground or the DLR.");
    });
  });

  describe("When a request is received", function () {

    describe("Given there are no disruptions", function () {

      var request;
      var response;

      beforeEach(function (done) {

        request = {};
        response = {};

        simple.mock(response, "card");
        simple.mock(response, "say");

        response.card.returnWith(response);
        response.say.returnWith(response);

        simple
          .mock(intent.api, "getDisruption")
          .resolveWith([]);

        intent
          .handler(request, response)
          .then(done);
      });

      it("Then the response is correct", function () {
        assert.equal(response.say.callCount, 1);
        assert.equal(response.say.lastCall.arg, "There is currently no disruption on the tube, London Overground or the D.L.R..");
      });
      it("Then a card is returned", function () {
        assert.equal(response.card.callCount, 1);
      });

      afterEach(function () {
        simple.restore();
      });
    });

    describe("Given there is a single disruption", function () {

      var request;
      var response;

      beforeEach(function (done) {

        request = {};
        response = {};

        simple.mock(response, "card");
        simple.mock(response, "say");

        response.card.returnWith(response);
        response.say.returnWith(response);

        simple
          .mock(intent.api, "getDisruption")
          .resolveWith([
            { description: "Disruption 1" },
            { description: "Disruption 2" },
            { description: "Disruption 2" }
          ]);

        intent
          .handler(request, response)
          .then(done);
      });

      it("Then the response is correct", function () {
        assert.equal(response.say.callCount, 1);
        assert.equal(response.say.lastCall.arg, "<p>Disruption 1</p> <p>Disruption 2</p> <p>There is a good service on all other lines.</p>");
      });
      it("Then a card is returned", function () {
        assert.equal(response.card.callCount, 1);
      });

      afterEach(function () {
        simple.restore();
      });
    });

    describe("Given an error occurs", function () {

      var request;
      var response;

      beforeEach(function (done) {

        request = {};
        response = {};

        simple.mock(response, "say");
        simple.mock(console, "error");

        response.say.returnWith(response);

        simple
          .mock(intent.api, "getDisruption")
          .rejectWith("An error");

        intent
          .handler(request, response)
          .then(done);
      });

      it("Then the response is correct", function () {
        assert.equal(response.say.callCount, 1);
        assert.equal(response.say.lastCall.arg, "Sorry, something went wrong.");
      });
      it("Then the error is logged", function () {
        assert.equal(console.error.callCount, 1);
        assert.equal(console.error.lastCall.args[0], "Failed to check for disruption:");
        assert.equal(console.error.lastCall.args[1], "An error");
      });

      afterEach(function () {
        simple.restore();
      });
    });
  });
});
