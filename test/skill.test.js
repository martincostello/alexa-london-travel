// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

var assert = require("assert");
var simple = require("simple-mock");
var skill = require("../src/skill.js");

describe("Skill", function () {

  describe("When an error occurs", function () {

    var response;

    beforeEach(function () {

      var request = {};
      response = {};

      simple.mock(console, "error");
      simple.mock(response, "say");

      response.say.returnWith(response);

      skill.onError("My error", request, response);
    });

    it("Then the exception is logged", function () {
      assert.equal(console.error.callCount, 1);
      assert.equal(console.error.lastCall.args[0], "Unhandled exception: ");
      assert.equal(console.error.lastCall.args[1], "My error");
    });

    it("Then the response is correct", function () {
      assert.equal(response.say.callCount, 1);
      assert.equal(response.say.lastCall.arg, "Sorry, something went wrong.");
    });
  });
});
