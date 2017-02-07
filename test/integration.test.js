// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

var app = require("../index.js");
var assert = require("assert");

describe("Integration", function () {

  describe("When the skill is launched", function () {

    var actual;

    beforeEach(function (done) {

      var json = {
        "version": "1.0",
        "session": {
          "new": true,
          "sessionId": "amzn1.echo-api.session.abeee1a7-aee0-41e6-8192-e6faaed9f5ef",
          "application": {
            "applicationId": null
          },
          "attributes": {},
          "user": {
            "userId": "amzn1.account.AM3B227HF3FAM1B261HK7FFM3A2"
          }
        },
        "context": {
          "System": {
            "application": {
              "applicationId": null
            },
            "user": {
              "userId": "amzn1.account.AM3B227HF3FAM1B261HK7FFM3A2"
            },
            "device": {
              "supportedInterfaces": {
                "AudioPlayer": {}
              }
            }
          },
          "AudioPlayer": {
            "offsetInMilliseconds": 0,
            "playerActivity": "IDLE"
          }
        },
        "request": {
          "type": "LaunchRequest",
          "requestId": "amzn1.echo-api.request.9cdaa4db-f20e-4c58-8d01-c75322d6c423",
          "timestamp": "2015-05-13T12:34:56Z"
        }
      };

      app.request(json).then(function (response) {
        actual = response;
        done();
      });
    });

    it("Then a response is returned", function () {
      assert.notEqual(actual, null);
      assert.notEqual(actual.response, null);
    });
    it("Then the session is not ended", function () {
      assert.equal(actual.response.shouldEndSession, false);
    });
    it("Then the output is correct", function () {
      assert.notEqual(actual.response.outputSpeech, null);
      assert.equal(actual.response.outputSpeech.type, "SSML");
      assert.equal(actual.response.outputSpeech.ssml, "<speak>Welcome to London Travel. You can ask me about disruption or for the status of any tube line, London Overground or the D.L.R..</speak>");
    });
  });

  describe("When the session is ended", function () {

    var actual;

    beforeEach(function (done) {

      var json = {
        "version": "1.0",
        "session": {
          "new": true,
          "sessionId": "amzn1.echo-api.session.abeee1a7-aee0-41e6-8192-e6faaed9f5ef",
          "application": {
            "applicationId": null
          },
          "attributes": {},
          "user": {
            "userId": "amzn1.account.AM3B227HF3FAM1B261HK7FFM3A2"
          }
        },
        "context": {
          "System": {
            "application": {
              "applicationId": null
            },
            "user": {
              "userId": "amzn1.account.AM3B227HF3FAM1B261HK7FFM3A2"
            },
            "device": {
              "supportedInterfaces": {
                "AudioPlayer": {}
              }
            }
          },
          "AudioPlayer": {
            "offsetInMilliseconds": 0,
            "playerActivity": "IDLE"
          }
        },
        "request": {
          "type": "SessionEndedRequest",
          "requestId": "amzn1.echo-api.request.9cdaa4db-f20e-4c58-8d01-c75322d6c423",
          "timestamp": "2015-05-13T12:34:56Z"
        }
      };

      app.request(json).then(function (response) {
        actual = response;
        done();
      });
    });

    it("Then a response is returned", function () {
      assert.notEqual(actual, null);
      assert.notEqual(actual.response, null);
    });
    it("Then the session is ended", function () {
      assert.equal(actual.response.shouldEndSession, true);
    });
    it("Then the output is correct", function () {
      assert.notEqual(actual.response.outputSpeech, null);
      assert.equal(actual.response.outputSpeech.type, "SSML");
      assert.equal(actual.response.outputSpeech.ssml, "<speak>Goodbye.</speak>");
    });
  });

  describe("When the application Id is incorrect.", function () {

    var actual;

    beforeEach(function (done) {

      var json = {
        "version": "1.0",
        "session": {
          "new": true,
          "sessionId": "amzn1.echo-api.session.abeee1a7-aee0-41e6-8192-e6faaed9f5ef",
          "application": {
            "applicationId": "not my application id"
          },
          "attributes": {},
          "user": {
            "userId": "amzn1.account.AM3B227HF3FAM1B261HK7FFM3A2"
          }
        },
        "context": {
          "System": {
            "application": {
              "applicationId": "not my application id"
            },
            "user": {
              "userId": "amzn1.account.AM3B227HF3FAM1B261HK7FFM3A2"
            },
            "device": {
              "supportedInterfaces": {
                "AudioPlayer": {}
              }
            }
          },
          "AudioPlayer": {
            "offsetInMilliseconds": 0,
            "playerActivity": "IDLE"
          }
        },
        "request": {
          "type": "SessionEndedRequest",
          "requestId": "amzn1.echo-api.request.9cdaa4db-f20e-4c58-8d01-c75322d6c423",
          "timestamp": "2015-05-13T12:34:56Z"
        }
      };

      app.request(json).catch(function (error) {
        actual = error;
        done();
      });
    });

    it("Then the request fails", function () {
      assert.equal(actual, "Invalid application Id.");
    });
  });
});
