// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var assert = require("assert");
var dataDriven = require("data-driven");
var verbalizer = require("../src/verbalizer");

describe("Verbalizer", function () {

  describe("When verbalizing text", function () {

    var testCases = [
      { text: "This is a potato.", expected: "This is a potato." },
      { text: "There are delays on the DLR today.", expected: "There are delays on the D.L.R. today." }
    ];

    dataDriven(testCases, function () {
      it("Then the spoken form is correct for '{text}'", function (context) {
        var actual = verbalizer.verbalize(context.text);
        assert.equal(actual, context.expected);
      });
    });
  });
});
