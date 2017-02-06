// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

var assert = require("assert");
var dataDriven = require("data-driven");
var intent = require("../../src/intents/disruption");

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
      assert.equal(actual, "There is currently no disruption on the tube, London Overground or the D.L.R.");
    });
  });

  describe("When there are no disuptions", function () {

    var data;
    var actual;

    beforeEach(function () {
      data = [];
      actual = null;
    });

    it("Then the response is that there are no disruptions", function () {
      actual = intent.generateResponse(data);
      assert.equal(actual, "There is currently no disruption on the tube, London Overground or the D.L.R.");
    });
  });

  describe("When there is one disruption", function () {

    var testCases = [
      { description: "There are severe delays on the District Line.", expected: "There are severe delays on the District Line." },
      { description: "There are minor delays on the Hammersmith & City Line.", expected: "There are minor delays on the Hammersmith and City Line." }
    ];

    dataDriven(testCases, function () {
      it("Then the response is the description of the single disruption", function (context) {

        var data = [
          { description: context.description }
        ];

        var actual = intent.generateResponse(data);
        assert.equal(actual, context.expected);
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
      actual = intent.generateResponse(data);
      assert.equal(actual, "There are severe delays on the District Line.\nThere are minor delays on the Circle Line.");
    });
  });

  describe("When generating a card", function () {

    var actual;

    beforeEach(function () {
      actual = intent.generateCard("There are no delays on the London Underground or the D.L.R..");
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
      assert.equal(actual.text, "There are no delays on the London Underground or the DLR.");
    });
  });
});
