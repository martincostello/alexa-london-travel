// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

var assert = require("assert");
var dataDriven = require("data-driven");
var intent = require("../../src/intents/status");

describe("Status Intent", function () {

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
      { name: "London Overground", expected: "london-overground" },
      { name: "Overground", expected: "london-overground" },
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
        { severity: 10, expected: "There is a good service on the District line." },
        { severity: 10, expected: "There is a good service on the D.L.R..", name: "DLR" },
        { severity: 10, expected: "There is a good service on London Overground.", name: "London Overground" },
        { severity: 10, expected: "There is a good service on the Waterloo and City line.", name: "Waterloo & City" },
        { severity: 18, expected: "There is a good service on the District line." },
        { severity: 20, expected: "The District line is closed." },
        { severity: 0, expected: "There is a special service." },
        { severity: 1, expected: "Blackfriars station is closed." },
        { severity: 2, expected: "The District line is suspended between Earls Court and Edgeware Road." },
        { severity: 3, expected: "The District line is partially suspended between Wimbledon and Earls Court." },
        { severity: 4, expected: "There is a planned closure on the District line between Richmond and Embankment." },
        { severity: 5, expected: "The District line is partially closed between Tower Hill and Barking." },
        { severity: 6, expected: "There are severe delays on the District line between Ealing Broadway and Turnham Green." },
        { severity: 7, expected: "There is a reduced service on the District line due to a shortage of train drivers." },
        { severity: 8, expected: "Some parts of the District line are currently being served by a replacement bus service." },
        { severity: 9, expected: "There are minor delays on the District line due to late-running engineering works." },
        { severity: 11, expected: "The District line is partially closed between Wimbledon and Edgware Road." },
        { severity: 12, expected: "Covent Garden station is exit-only." },
        { severity: 13, expected: "No step-free access." },
        { severity: 14, expected: "The frequency of service has been changed." },
        { severity: 15, expected: "The service has been diverted." },
        { severity: 16, expected: "The District line is closed." },
        { severity: 17, expected: "The service is not running." },
        { severity: 19, expected: "This is some information." },
        { severity: 20, expected: "The District line is closed." },
        { severity: 99, expected: "There is currently disruption on the District line." },
      ];

      dataDriven(testCases, function () {
        it("Then the response is correct for severity {severity}", function (context) {

          var data = [
            {
              name: context.name || "District",
              lineStatuses: [
                {
                  statusSeverity: context.severity,
                  reason: context.reason || context.expected
                }
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

    describe("When generating a card", function () {

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
          var actual = intent.generateCard(context.name, context.text);
          assert.notEqual(actual, null);
          assert.equal(actual.type, "Standard");
          assert.equal(actual.title, context.expectedTitle);
          assert.equal(actual.text, context.expectedText);
        });
      });
    });
  });
});
