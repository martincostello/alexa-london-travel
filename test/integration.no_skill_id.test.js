// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

delete require.cache[require.resolve("../index")];
var app = require("../index");
var assert = require("assert");

describe("Integration", function () {
  it("Then the application Id is not set", function () {
    assert.equal(app.id, undefined);
  });
});
