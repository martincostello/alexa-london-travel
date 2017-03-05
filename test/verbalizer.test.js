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
      { text: "There are delays on the DLR today.", expected: "There are delays on the D.L.R. today." },
      { text: "There are delays e/b on the DLR.", expected: "There are delays eastbound on the D.L.R.." },
      { text: "There are delays n/b on the DLR.", expected: "There are delays northbound on the D.L.R.." },
      { text: "There are delays s/b on the DLR.", expected: "There are delays southbound on the D.L.R.." },
      { text: "There are delays w/b on the DLR.", expected: "There are delays westbound on the D.L.R.." },
      { text: "Tickets will be accepted on SWT.", expected: "Tickets will be accepted on South West Trains." }
    ];

    dataDriven(testCases, function () {
      it("Then the spoken form is correct for '{text}'", function (context) {
        var actual = verbalizer.verbalize(context.text);
        assert.equal(actual, context.expected);
      });
    });
  });
});
