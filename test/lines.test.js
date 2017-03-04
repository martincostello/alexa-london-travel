// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

var assert = require("assert");
var dataDriven = require("data-driven");
var lines = require("../src/lines");
var simple = require("simple-mock");

describe("Lines", function () {

  describe("When coverting line names to their spoken form", function () {

    var testCases = [
      { name: "Bakerloo", expected: "the Bakerloo line" },
      { name: "Central", expected: "the Central line" },
      { name: "Circle", expected: "the Circle line" },
      { name: "District", expected: "the District line" },
      { name: "DLR", expected: "the D.L.R." },
      { name: "Hammersmith & City", expected: "the Hammersmith & City line" },
      { name: "Jubilee", expected: "the Jubilee line" },
      { name: "London Overground", expected: "London Overground" },
      { name: "Metropolitan", expected: "the Metropolitan line" },
      { name: "Northern", expected: "the Northern line" },
      { name: "Piccadilly", expected: "the Piccadilly line" },
      { name: "Victoria", expected: "the Victoria line" },
      { name: "Waterloo & City", expected: "the Waterloo & City line" },
    ];

    dataDriven(testCases, function () {
      it("Then the spoken form is correct for '{name}'", function (context) {
        var actual = lines.toSpokenName(context.name);
        assert.equal(actual, context.expected);
      });
    });
  });

  describe("When testing for the DLR", function () {

    var testCases = [
      { name: "bakerloo", expected: false },
      { name: "central", expected: false },
      { name: "circle", expected: false },
      { name: "district", expected: false },
      { name: "dlr", expected: true },
      { name: "DLR", expected: true },
      { name: "hammersmith-city", expected: false },
      { name: "jubilee", expected: false },
      { name: "london-overground", expected: false },
      { name: "metropolitan", expected: false },
      { name: "northern", expected: false },
      { name: "piccadilly", expected: false },
      { name: "victoria", expected: false },
      { name: "waterloo-city", expected: false },
    ];

    dataDriven(testCases, function () {
      it("Then result is correct for '{name}'", function (context) {
        var actual = lines.isDLR(context.name);
        assert.equal(actual, context.expected);
      });
    });
  });

  describe("When testing for London Overground", function () {

    var testCases = [
      { name: "bakerloo", expected: false },
      { name: "central", expected: false },
      { name: "circle", expected: false },
      { name: "district", expected: false },
      { name: "dlr", expected: false },
      { name: "hammersmith-city", expected: false },
      { name: "jubilee", expected: false },
      { name: "london-overground", expected: true },
      { name: "London Overground", expected: true },
      { name: "metropolitan", expected: false },
      { name: "northern", expected: false },
      { name: "piccadilly", expected: false },
      { name: "victoria", expected: false },
      { name: "waterloo-city", expected: false },
    ];

    dataDriven(testCases, function () {
      it("Then result is correct for '{name}'", function (context) {
        var actual = lines.isOverground(context.name);
        assert.equal(actual, context.expected);
      });
    });
  });
});
