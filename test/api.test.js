// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var api = require("../src/api.js");
var assert = require("assert");
var nock = require("nock");

describe("TfL API", function () {

  beforeEach(function () {
    api.appId = "MyApplicationId";
    api.appKey = "MyApplicationKey";
  });

  describe("When requesting disruption", function () {

    var actual;

    beforeEach(function (done) {

      nock("https://api.tfl.gov.uk")
        .get("/Line/Mode/tube/Disruption")
        .query({ app_id: "MyApplicationId", app_key: "MyApplicationKey" })
        .reply(200, []);

      api.getDisruption(["tube"])
        .then(function (response) {
          actual = response;
          done();
        });
    });

    it("Then the correct response is returned", function () {
      assert.deepEqual(actual, []);
    });
  });

  describe("When requesting the status of a line", function () {

    var actual;

    beforeEach(function (done) {

      nock("https://api.tfl.gov.uk")
        .get("/Line/victoria/Status")
        .query({ app_id: "MyApplicationId", app_key: "MyApplicationKey" })
        .reply(200, []);

      api.getLineStatus("victoria")
        .then(function (response) {
          actual = response;
          done();
        });
    });

    it("Then the correct response is returned", function () {
      assert.deepEqual(actual, []);
    });
  });
});
