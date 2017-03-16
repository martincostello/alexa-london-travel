// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var assert = require("assert");
var cards = require("../src/cards");
var dataDriven = require("data-driven");

describe("When generating cards", function () {

  describe("When generating a disruption card", function () {

    var actual;

    beforeEach(function () {
      var statuses = [];
      actual = cards.forDisruption(statuses);
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

  describe("When generating a status intent card", function () {

    var testCases = [
      {
        name: "DLR",
        text: "There is a good status on the D.L.R..",
        expectedTitle: "DLR Status",
        expectedText: "There is a good status on the DLR."
      },
      {
        name: "London Overground",
        text: "There is a good status on the London Overground.",
        expectedTitle: "London Overground Status",
        expectedText: "There is a good status on the London Overground."
      },
      {
        name: "Waterloo & City",
        text: "There are minor delays on the Waterloo and City line.",
        expectedTitle: "Waterloo & City Line Status",
        expectedText: "There are minor delays on the Waterloo and City line."
      }
    ];

    dataDriven(testCases, function () {

      it("Then the card is generated correctly", function (context) {
        var actual = cards.forStatus(context.name, context.text);
        assert.notEqual(actual, null);
        assert.equal(actual.type, "Standard");
        assert.equal(actual.title, context.expectedTitle);
        assert.equal(actual.text, context.expectedText);
      });
    });
  });
});
