// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var assert = require("assert");
var nock = require("nock");
var skillApi = require("../src/skillApi");

describe("Skill API", function () {

  describe("When requesting preferences with a valid access token", function () {

    var actual;
    var accessToken;
    var expected;

    beforeEach(function (done) {

      accessToken = "MyAccessToken";

      expected = {
        favoriteLines: ["district", "victoria"],
        userId: "MyUserId"
      };

      nock("https://londontravel.martincostello.com/", {
        reqheaders: {
          "Authorization": "Bearer MyAccessToken"
        }
      })
        .get("/api/preferences")
        .reply(200, expected);

      skillApi.getPreferences(accessToken)
        .then(function (response) {
          actual = response;
          done();
        });
    });

    it("Then the correct response is returned", function () {
      assert.deepEqual(actual, expected);
    });
  });

  describe("When requesting preferences with an invalid access token", function () {

    var actual;
    var accessToken;
    var expected;

    beforeEach(function (done) {

      accessToken = "MyAccessToken";

      nock("https://londontravel.martincostello.com/", {
        reqheaders: {
          "Authorization": "Bearer MyAccessToken"
        }
      })
        .get("/api/preferences")
        .reply(401, {
          statusCode: 401,
          message: "Unauthorized.",
          requestId: "SomeRequestId"
        });

      skillApi.getPreferences(accessToken)
        .then(function (response) {
          actual = response;
          done();
        });
    });

    it("Then the correct response is returned", function () {
      assert.equal(actual, null);
    });
  });

  describe("When requesting preferences if an error occurs", function () {

    var actual;
    var accessToken;
    var expected;

    beforeEach(function (done) {

      accessToken = "MyAccessToken";

      nock("https://londontravel.martincostello.com/", {
        reqheaders: {
          "Authorization": "Bearer MyAccessToken"
        }
      })
        .get("/api/preferences")
        .reply(500, {
          statusCode: 500,
          message: "Internal server error.",
          requestId: "SomeRequestId"
        });

      skillApi.getPreferences(accessToken)
        .catch(function (error) {
          actual = error;
          done();
        });
    });

    it("Then the correct error is thrown", function () {
      assert.deepEqual(actual, new Error("Failed to get user preferences using the London Travel Skill API: 500"));
    });
  });
});
