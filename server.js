// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

var AlexaAppServer = require("alexa-app-server");
var env = require("node-env-file");

env(__dirname + "/.env");

console.log("TfL App Id :", process.env.TFL_APP_ID);
console.log("TfL App Key:", process.env.TFL_APP_KEY);

AlexaAppServer.start({
  server_root: __dirname,
  public_html: "static",
  app_dir: "./..",
  app_root: "/",
  port: 3001,
  preRequest: function (json, request, response) {
  },
  postRequest: function (json, request, response) {
  }
});
