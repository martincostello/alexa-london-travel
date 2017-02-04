// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

var assert = require("assert");
var dataDriven = require("data-driven");
var intent = require("../../src/intents/status");

describe("Status Intent", function () {

  describe("When coverting line names to their spoken form", function () {

    var testCases = [
      { name: "Bakerloo", expected: "Bakerloo line" },
      { name: "Central", expected: "Central line" },
      { name: "Circle", expected: "Circle line" },
      { name: "District", expected: "District line" },
      { name: "DLR", expected: "D.L.R." },
      { name: "Hammersmith & City", expected: "Hammersmith & City line" },
      { name: "Jubilee", expected: "Jubilee line" },
      { name: "Metropolitan", expected: "Metropolitan line" },
      { name: "Northern", expected: "Northern line" },
      { name: "Piccadilly", expected: "Piccadilly line" },
      { name: "Victoria", expected: "Victoria line" },
      { name: "Waterloo & City", expected: "Waterloo & City line" },
    ];

    dataDriven(testCases, function () {
      it("Then the spoken form is correct for '{name}'", function (context) {
        var actual = intent.toSpokenLineName(context.name);
        assert.equal(actual, context.expected);
      });
    });
  });

  describe("When mapping line names", function () {

    var testCases = [
      { name: null, expected: null },
      { name: "", expected: null },
      { name: "  ", expected: null },
      { name: "not a tube line", expected: null },
      { name: "Bakerloo", expected: "bakerloo" },
      { name: "bakerloo", expected: "bakerloo" },
      { name: "BAKERLOO", expected: "bakerloo" },
      { name: "Central", expected: "central" },
      { name: "Circle", expected: "circle" },
      { name: "District", expected: "district" },
      { name: "DLR", expected: "dlr" },
      { name: "Docklands", expected: "dlr" },
      { name: "Docklands Light Railway", expected: "dlr" },
      { name: "Docklands Railway", expected: "dlr" },
      { name: "Hammersmith", expected: "hammersmith-city" },
      { name: "Hammersmith & City", expected: "hammersmith-city" },
      { name: "Hammersmith and City", expected: "hammersmith-city" },
      { name: "Jubilee", expected: "jubilee" },
      { name: "Met", expected: "metropolitan" },
      { name: "Metropolitan", expected: "metropolitan" },
      { name: "Northern", expected: "northern" },
      { name: "Piccadilly", expected: "piccadilly" },
      { name: "Victoria", expected: "victoria" },
      { name: "City", expected: "waterloo-city" },
      { name: "Waterloo", expected: "waterloo-city" },
      { name: "Waterloo & City", expected: "waterloo-city" },
      { name: "Waterloo and City", expected: "waterloo-city" }
    ];

    dataDriven(testCases, function () {
      it("Then the mapped Id is correct for '{name}'", function (context) {
        var actual = intent.mapLineToId(context.name);
        assert.equal(actual, context.expected);
      });
    });
  });

  describe("When generating responses", function () {

    describe("Given there is a valid status response with only one disruption", function () {

      var testCases = [
        { severity: 0, expected: "There is currently disruption on the District line." },
        { severity: 1, expected: "The District line is closed." },
        { severity: 2, expected: "The District line is suspended." },
        { severity: 3, expected: "The District line is partially suspended." },
        { severity: 4, expected: "There is a planned closure on the District line." },
        { severity: 5, expected: "The District line is partially closed." },
        { severity: 6, expected: "There are severe delays on the District line." },
        { severity: 7, expected: "There is a reduced service on the District line." },
        { severity: 8, expected: "Some parts of the District line are currently being served by a replacement bus service." },
        { severity: 9, expected: "There are minor delays on the District line." },
        { severity: 10, expected: "There is a good service on the District line." },
        { severity: 10, expected: "There is a good service on the D.L.R..", name: "DLR" },
        { severity: 10, expected: "There is a good service on the Waterloo and City line.", name: "Waterloo & City" },
        { severity: 11, expected: "The District line is partially closed." },
        { severity: 12, expected: "There is currently disruption on the District line." },
        { severity: 13, expected: "There is currently disruption on the District line." },
        { severity: 14, expected: "There is currently disruption on the District line." },
        { severity: 15, expected: "There is currently disruption on the District line." },
        { severity: 16, expected: "The District line is closed." },
        { severity: 17, expected: "There is currently disruption on the District line." },
        { severity: 18, expected: "There is a good service on the District line." },
        { severity: 19, expected: "There is currently disruption on the District line." },
        { severity: 20, expected: "The District line is closed." },
        { severity: 99, expected: "There is currently disruption on the District line." },
      ];

      dataDriven(testCases, function () {
        it("Then the response is correct for severity {severity}", function (context) {

          var data = [
            {
              name: context.name || "District",
              lineStatuses: [
                { statusSeverity: context.severity }
              ]
            }
          ];

          var actual = intent.generateResponse(data);
          assert.equal(actual, context.expected);
        });
      });
    });

    describe("Given there is a valid status response with multiple disruptions", function () {

      var testCases = [
        {
          statuses: [
            { statusSeverity: 5, reason: "HAMMERSMITH & CITY LINE: Saturday 4 and Sunday 5 February, no service between Liverpool Street and Barking." },
            { statusSeverity: 6, reason: "Hammersmith and City Line: No service between Liverpool Street and Barking due to planned engineering work. SEVERE DELAYS on the rest of the line." }
          ],
          expected: "No service between Liverpool Street and Barking due to planned engineering work. SEVERE DELAYS on the rest of the line."
        },
        {
          statuses: [
            { statusSeverity: 5, reason: "Part closure." },
            { statusSeverity: 6, reason: "Minor delays." }
          ],
          expected: "Minor delays."
        }
      ];

      dataDriven(testCases, function () {
        it("Then the response is correct", function (context) {

          var data = [
            {
              name: "Hammersmith & City",
              lineStatuses: context.statuses
            }
          ];

          var actual = intent.generateResponse(data);
          assert.equal(actual, context.expected);
        });
      });
    });

    describe("Given there is an invalid status response", function () {

      var expected = "Sorry, something went wrong.";

      var testCases = [
        { data: null },
        { data: [] },
        { data: [{}] },
        { data: [{ lineStatuses: null }] },
        { data: [{ lineStatuses: [] }] },
        { data: [{ lineStatuses: [{ statusSeverity: null }] }] }
      ];

      dataDriven(testCases, function () {
        it("Then the invalid data is handled correctly", function (context) {
          var actual = intent.generateResponse(context.data);
          assert.equal(actual, "Sorry, something went wrong.");
        });
      });
    });
  });
});
