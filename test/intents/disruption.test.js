// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

var assert = require("assert");
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
      assert.equal(actual, "There is currently no disruption on the tube or the DLR.");
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
      assert.equal(actual, "There is currently no disruption on the tube or the DLR.");
    });
  });

  describe("When there is one disruption", function () {

    var data;
    var actual;

    beforeEach(function () {

      data = [
        { description: "There are severe delays on the District Line." }
      ];

      actual = null;
    });

    it("Then the response is the description of the single disruption", function () {
      actual = intent.generateResponse(data);
      assert.equal(actual, "There are severe delays on the District Line.");
    });
  });

  describe("When there are multiple disruptions", function () {

    var data;
    var actual;

    beforeEach(function () {

      data = [
        { description: "There are severe delays on the District Line." },
        { description: "There are minor delays on the Circle Line." }
      ];

      actual = null;
    });

    it("Then the response is the description of the single disruption", function () {
      actual = intent.generateResponse(data);
      assert.equal(actual, "There are severe delays on the District Line.");
    });
  });
});
