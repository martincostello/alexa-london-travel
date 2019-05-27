// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

process.env.SKILL_ID = "MySkillId";
process.env.VERIFY_SKILL_ID = "true";

delete require.cache[require.resolve("../index")];
var app = require("../index");
var assert = require("assert");
var helpers = require("./helpers");
var mock = require("./mock");
var tflApi = require("../src/tflApi");

describe("Integration", function () {

  it("Then the application Id is set", function () {
    assert.equal(app.id, "MySkillId");
  });

  describe("When the skill is launched", function () {

    var actual;

    beforeEach(function (done) {

      var json = helpers.launchRequest();

      app.request(json).then(function (response) {
        actual = response;
        done();
      });
    });

    it("Then there is a response", function () {
      assert.notEqual(actual, null);
      assert.notEqual(actual.response, null);
    });
    it("Then the session does not end", function () {
      assert.equal(actual.response.shouldEndSession, false);
    });
    it("Then the speech is correct", function () {
      assert.notEqual(actual.response.outputSpeech, null);
      assert.equal(actual.response.outputSpeech.type, "SSML");
      assert.equal(actual.response.outputSpeech.ssml, "<speak>Welcome to London Travel. You can ask me about disruption or for the status of any tube line, London Overground, the D.L.R. or T.F.L. Rail.</speak>");
    });
  });

  describe("When the commute intent is requested", function () {

    describe("Given there is no session", function () {

      var json;
      var actual;

      beforeEach(function (done) {
        json = helpers.commuteRequest();
        delete json.session;
        app.request(json).then(function (response) {
          actual = response;
          done();
        });
      });

      it("Then there is a response", function () {
        assert.notEqual(actual, null);
        assert.notEqual(actual.response, null);
      });
      it("Then the session ends", function () {
        assert.equal(actual.response.shouldEndSession, true);
      });
      it("Then the speech is correct", function () {
        assert.notEqual(actual.response.outputSpeech, null);
        assert.equal(actual.response.outputSpeech.type, "SSML");
        assert.equal(actual.response.outputSpeech.ssml, "<speak>You need to link your account to be able to ask me about your commute.</speak>");
      });
      it("Then the card is correct", function () {
        assert.notEqual(actual.response.card, null);
        assert.equal(actual.response.card.type, "LinkAccount");
      });
    });

    describe("Given there is no access token", function () {

      var json;
      var actual;

      beforeEach(function (done) {
        json = helpers.commuteRequest();
        app.request(json).then(function (response) {
          actual = response;
          done();
        });
      });

      it("Then there is a response", function () {
        assert.notEqual(actual, null);
        assert.notEqual(actual.response, null);
      });
      it("Then the session ends", function () {
        assert.equal(actual.response.shouldEndSession, true);
      });
      it("Then the speech is correct", function () {
        assert.notEqual(actual.response.outputSpeech, null);
        assert.equal(actual.response.outputSpeech.type, "SSML");
        assert.equal(actual.response.outputSpeech.ssml, "<speak>You need to link your account to be able to ask me about your commute.</speak>");
      });
      it("Then the card is correct", function () {
        assert.notEqual(actual.response.card, null);
        assert.equal(actual.response.card.type, "LinkAccount");
      });
    });

    describe("Given there is an access token", function () {

      var accessToken;

      describe("Given the access token is invalid", function () {

        var json;
        var actual;

        beforeEach(function (done) {

          accessToken = "NotAValidToken";
          json = helpers.commuteRequest(accessToken);

          mock.skill.unauthorized(accessToken);

          app.request(json).then(function (response) {
            actual = response;
            done();
          });
        });

        it("Then there is a response", function () {
          assert.notEqual(actual, null);
          assert.notEqual(actual.response, null);
        });
        it("Then the session ends", function () {
          assert.equal(actual.response.shouldEndSession, true);
        });
        it("Then the speech is correct", function () {
          assert.notEqual(actual.response.outputSpeech, null);
          assert.equal(actual.response.outputSpeech.type, "SSML");
          assert.equal(actual.response.outputSpeech.ssml, "<speak>It looks like you've disabled account linking. You need to re-link your account to be able to ask me about your commute.</speak>");
        });
        it("Then the card is correct", function () {
          assert.notEqual(actual.response.card, null);
          assert.equal(actual.response.card.type, "LinkAccount");
        });
      });

      describe("Given the access token is valid", function () {

        var json;

        beforeEach(function () {
          accessToken = "AValidToken";
          json = helpers.commuteRequest(accessToken);
        });

        describe("Given the user has set no preferences (no locale)", function () {

          var actual;

          beforeEach(function (done) {

            mock.skill.success(accessToken, {
              favoriteLines: []
            });

            delete json.request.locale;

            app.request(json).then(function (response) {
              actual = response;
              done();
            });
          });

          it("Then there is a response", function () {
            assert.notEqual(actual, null);
            assert.notEqual(actual.response, null);
          });
          it("Then the session ends", function () {
            assert.equal(actual.response.shouldEndSession, true);
          });
          it("Then the speech is correct", function () {
            assert.notEqual(actual.response.outputSpeech, null);
            assert.equal(actual.response.outputSpeech.type, "SSML");
            assert.equal(actual.response.outputSpeech.ssml, "<speak>You have not selected any favourite lines yet. Visit the London Travel website to set your preferences.</speak>");
          });
          it("Then the card is correct", function () {
            assert.notEqual(actual.response.card, null);
            assert.equal(actual.response.card.type, "Standard");
            assert.equal(actual.response.card.title, "Your Commute");
            assert.equal(actual.response.card.text, "You have not selected any favourite lines yet. Visit the London Travel website to set your preferences.");
          });
        });

        describe("Given the user has set no preferences (en-GB)", function () {

          var actual;

          beforeEach(function (done) {

            mock.skill.success(accessToken, {
              favoriteLines: []
            });

            json.request.locale = "en-GB";

            app.request(json).then(function (response) {
              actual = response;
              done();
            });
          });

          it("Then there is a response", function () {
            assert.notEqual(actual, null);
            assert.notEqual(actual.response, null);
          });
          it("Then the session ends", function () {
            assert.equal(actual.response.shouldEndSession, true);
          });
          it("Then the speech is correct", function () {
            assert.notEqual(actual.response.outputSpeech, null);
            assert.equal(actual.response.outputSpeech.type, "SSML");
            assert.equal(actual.response.outputSpeech.ssml, "<speak>You have not selected any favourite lines yet. Visit the London Travel website to set your preferences.</speak>");
          });
          it("Then the card is correct", function () {
            assert.notEqual(actual.response.card, null);
            assert.equal(actual.response.card.type, "Standard");
            assert.equal(actual.response.card.title, "Your Commute");
            assert.equal(actual.response.card.text, "You have not selected any favourite lines yet. Visit the London Travel website to set your preferences.");
          });
        });

        describe("Given the user has set no preferences (en-US)", function () {

          var actual;

          beforeEach(function (done) {

            mock.skill.success(accessToken, {});

            json.request.locale = "en-US";

            app.request(json).then(function (response) {
              actual = response;
              done();
            });
          });

          it("Then there is a response", function () {
            assert.notEqual(actual, null);
            assert.notEqual(actual.response, null);
          });
          it("Then the session ends", function () {
            assert.equal(actual.response.shouldEndSession, true);
          });
          it("Then the speech is correct", function () {
            assert.notEqual(actual.response.outputSpeech, null);
            assert.equal(actual.response.outputSpeech.type, "SSML");
            assert.equal(actual.response.outputSpeech.ssml, "<speak>You have not selected any favorite lines yet. Visit the London Travel website to set your preferences.</speak>");
          });
          it("Then the card is correct", function () {
            assert.notEqual(actual.response.card, null);
            assert.equal(actual.response.card.type, "Standard");
            assert.equal(actual.response.card.title, "Your Commute");
            assert.equal(actual.response.card.text, "You have not selected any favorite lines yet. Visit the London Travel website to set your preferences.");
          });
        });

        describe("Given the user has set one preferred line but it is the Elizabeth line", function () {

          var actual;

          beforeEach(function (done) {

            mock.skill.success(accessToken, {
              favoriteLines: ["elizabeth"]
            });

            delete json.request.locale;

            app.request(json).then(function (response) {
              actual = response;
              done();
            });
          });

          it("Then there is a response", function () {
            assert.notEqual(actual, null);
            assert.notEqual(actual.response, null);
          });
          it("Then the session ends", function () {
            assert.equal(actual.response.shouldEndSession, true);
          });
          it("Then the speech is correct", function () {
            assert.notEqual(actual.response.outputSpeech, null);
            assert.equal(actual.response.outputSpeech.type, "SSML");
            assert.equal(actual.response.outputSpeech.ssml, "<speak>You have not selected any favourite lines yet. Visit the London Travel website to set your preferences.</speak>");
          });
          it("Then the card is correct", function () {
            assert.notEqual(actual.response.card, null);
            assert.equal(actual.response.card.type, "Standard");
            assert.equal(actual.response.card.title, "Your Commute");
            assert.equal(actual.response.card.text, "You have not selected any favourite lines yet. Visit the London Travel website to set your preferences.");
          });
        });

        describe("Given the user has set one preferred line", function () {

          var actual;

          beforeEach(function (done) {

            mock.skill.success(accessToken, {
              favoriteLines: [
                "district"
              ]
            });

            tflApi.appId = "MyApplicationId";
            tflApi.appKey = "MyApplicationKey";

            mock.tfl.success(
              "/Line/district/Status",
              [
                {
                  "id": "district",
                  "name": "District",
                  "lineStatuses": [
                    {
                      "statusSeverity": 10
                    }
                  ]
                }
              ]);

            app.request(json).then(function (response) {
              actual = response;
              done();
            });
          });

          it("Then there is a response", function () {
            assert.notEqual(actual, null);
            assert.notEqual(actual.response, null);
          });
          it("Then the session ends", function () {
            assert.equal(actual.response.shouldEndSession, true);
          });
          it("Then the speech is correct", function () {
            assert.notEqual(actual.response.outputSpeech, null);
            assert.equal(actual.response.outputSpeech.type, "SSML");
            assert.equal(actual.response.outputSpeech.ssml, "<speak>There is a good service on the District line.</speak>");
          });
          it("Then the card is correct", function () {
            assert.notEqual(actual.response.card, null);
            assert.equal(actual.response.card.type, "Standard");
            assert.equal(actual.response.card.title, "Your Commute");
            assert.equal(actual.response.card.text, "There is a good service on the District line.");
          });
        });

        describe("Given the user has set multiple preferences", function () {

          var actual;

          beforeEach(function (done) {

            mock.skill.success(accessToken, {
              favoriteLines: [
                "district",
                "waterloo-city"
              ]
            });

            tflApi.appId = "MyApplicationId";
            tflApi.appKey = "MyApplicationKey";

            mock.tfl.success(
              "/Line/district/Status",
              [
                {
                  "id": "district",
                  "name": "District",
                  "lineStatuses": [
                    {
                      "statusSeverity": 10
                    }
                  ]
                }
              ]);

            mock.tfl.success(
              "/Line/waterloo-city/Status",
              [
                {
                  "id": "waterloo-city",
                  "name": "Waterloo & City",
                  "lineStatuses": [
                    {
                      "statusSeverity": 6,
                      "reason": "Waterloo & City Line: SEVERE DELAYS due to a person ill on a train earlier at Waterloo.",
                      "disruption": {
                        "description": "Waterloo & City Line: SEVERE DELAYS due to a person ill on a train earlier at Waterloo."
                      }
                    }
                  ]
                }
              ]);

            app.request(json).then(function (response) {
              actual = response;
              done();
            });
          });

          it("Then there is a response", function () {
            assert.notEqual(actual, null);
            assert.notEqual(actual.response, null);
          });
          it("Then the session ends", function () {
            assert.equal(actual.response.shouldEndSession, true);
          });
          it("Then the speech is correct", function () {
            assert.notEqual(actual.response.outputSpeech, null);
            assert.equal(actual.response.outputSpeech.type, "SSML");
            assert.equal(actual.response.outputSpeech.ssml, "<speak><p>District Line: There is a good service on the District line.</p> <p>Waterloo &amp; City Line: SEVERE DELAYS due to a person ill on a train earlier at Waterloo.</p></speak>");
          });
          it("Then the card is correct", function () {
            assert.notEqual(actual.response.card, null);
            assert.equal(actual.response.card.type, "Standard");
            assert.equal(actual.response.card.title, "Your Commute");
            assert.equal(actual.response.card.text, "District Line: There is a good service on the District line.\nWaterloo & City Line: SEVERE DELAYS due to a person ill on a train earlier at Waterloo.");
          });
        });

        describe("Given the user has set multiple preferences but one is the Elizabeth line", function () {

          var actual;

          beforeEach(function (done) {

            mock.skill.success(accessToken, {
              favoriteLines: [
                "district",
                "elizabeth"
              ]
            });

            tflApi.appId = "MyApplicationId";
            tflApi.appKey = "MyApplicationKey";

            mock.tfl.success(
              "/Line/district/Status",
              [
                {
                  "id": "district",
                  "name": "District",
                  "lineStatuses": [
                    {
                      "statusSeverity": 10
                    }
                  ]
                }
              ]);

            app.request(json).then(function (response) {
              actual = response;
              done();
            });
          });

          it("Then there is a response", function () {
            assert.notEqual(actual, null);
            assert.notEqual(actual.response, null);
          });
          it("Then the session ends", function () {
            assert.equal(actual.response.shouldEndSession, true);
          });
          it("Then the speech is correct", function () {
            assert.notEqual(actual.response.outputSpeech, null);
            assert.equal(actual.response.outputSpeech.type, "SSML");
            assert.equal(actual.response.outputSpeech.ssml, "<speak>There is a good service on the District line.</speak>");
          });
          it("Then the card is correct", function () {
            assert.notEqual(actual.response.card, null);
            assert.equal(actual.response.card.type, "Standard");
            assert.equal(actual.response.card.title, "Your Commute");
            assert.equal(actual.response.card.text, "There is a good service on the District line.");
          });
        });
      });

      describe("Given an error occurs when calling the skill's API", function () {

        var json;
        var actual;

        beforeEach(function (done) {

          accessToken = "AValidToken";
          json = helpers.commuteRequest(accessToken);

          mock.skill.failure(accessToken);

          app.request(json).then(function (response) {
            actual = response;
            done();
          });
        });

        it("Then there is a response", function () {
          assert.notEqual(actual, null);
          assert.notEqual(actual.response, null);
        });
        it("Then the session ends", function () {
          assert.equal(actual.response.shouldEndSession, true);
        });
        it("Then the speech is correct", function () {
          assert.notEqual(actual.response.outputSpeech, null);
          assert.equal(actual.response.outputSpeech.type, "SSML");
          assert.equal(actual.response.outputSpeech.ssml, "<speak>Sorry, something went wrong.</speak>");
        });
      });

      describe("Given an error occurs when calling the TFL API", function () {

        var json;
        var actual;

        beforeEach(function (done) {

          accessToken = "AValidToken";
          json = helpers.commuteRequest(accessToken);

          mock.skill.success(accessToken, {
            favoriteLines: [
              "district",
              "waterloo-city"
            ]
          });

          tflApi.appId = "MyApplicationId";
          tflApi.appKey = "MyApplicationKey";

          mock.tfl.success(
            "/Line/district/Status",
            [
              {
                "id": "district",
                "name": "District",
                "lineStatuses": [
                  {
                    "statusSeverity": 10
                  }
                ]
              }
            ]);

          mock.tfl.failure("/Line/waterloo-city/Status");

          app.request(json).then(function (response) {
            actual = response;
            done();
          });
        });

        it("Then there is a response", function () {
          assert.notEqual(actual, null);
          assert.notEqual(actual.response, null);
        });
        it("Then the session ends", function () {
          assert.equal(actual.response.shouldEndSession, true);
        });
        it("Then the speech is correct", function () {
          assert.notEqual(actual.response.outputSpeech, null);
          assert.equal(actual.response.outputSpeech.type, "SSML");
          assert.equal(actual.response.outputSpeech.ssml, "<speak>Sorry, something went wrong.</speak>");
        });
      });
    });
  });

  describe("When the disruption intent is requested", function () {

    var json;

    beforeEach(function () {

      tflApi.appId = "MyApplicationId";
      tflApi.appKey = "MyApplicationKey";

      json = helpers.intentRequest("DisruptionIntent");
    });

    describe("Given there is no disruption", function () {

      var actual;

      beforeEach(function (done) {

        mock.tfl.success("/Line/Mode/dlr,overground,tube,tflrail/Disruption", []);

        app.request(json).then(function (response) {
          actual = response;
          done();
        });
      });

      it("Then there is a response", function () {
        assert.notEqual(actual, null);
        assert.notEqual(actual.response, null);
      });
      it("Then the session ends", function () {
        assert.equal(actual.response.shouldEndSession, true);
      });
      it("Then the speech is correct", function () {
        assert.notEqual(actual.response.outputSpeech, null);
        assert.equal(actual.response.outputSpeech.type, "SSML");
        assert.equal(actual.response.outputSpeech.ssml, "<speak>There is currently no disruption on the tube, London Overground, the D.L.R. or T.F.L. Rail.</speak>");
      });
      it("Then the card is correct", function () {
        assert.notEqual(actual.response.card, null);
        assert.equal(actual.response.card.type, "Standard");
        assert.equal(actual.response.card.title, "Disruption Summary");
        assert.equal(actual.response.card.text, "There is currently no disruption on the tube, London Overground, the DLR or TfL Rail.");
      });
    });

    describe("Given there is disruption", function () {

      var actual;

      beforeEach(function (done) {

        mock.tfl.success(
          "/Line/Mode/dlr,overground,tube,tflrail/Disruption",
          [
            {
              description: "DLR: Minor delays."
            },
            {
              description: "Waterloo & City line: Severe delays."
            },
            {
              description: "Waterloo & City line: Severe delays."
            }
          ]);

        app.request(json).then(function (response) {
          actual = response;
          done();
        });
      });

      it("Then there is a response", function () {
        assert.notEqual(actual, null);
        assert.notEqual(actual.response, null);
      });
      it("Then the session ends", function () {
        assert.equal(actual.response.shouldEndSession, true);
      });
      it("Then the speech is correct", function () {
        assert.notEqual(actual.response.outputSpeech, null);
        assert.equal(actual.response.outputSpeech.type, "SSML");
        assert.equal(actual.response.outputSpeech.ssml, "<speak><p>D.L.R.: Minor delays.</p> <p>Waterloo and City line: Severe delays.</p> <p>There is a good service on all other lines.</p></speak>");
      });
      it("Then the card is correct", function () {
        assert.notEqual(actual.response.card, null);
        assert.equal(actual.response.card.type, "Standard");
        assert.equal(actual.response.card.title, "Disruption Summary");
        assert.equal(actual.response.card.text, "DLR: Minor delays.\nWaterloo & City line: Severe delays.");
      });
    });

    describe("Given an error occurs", function () {

      var actual;

      beforeEach(function (done) {

        mock.tfl.failure("/Line/Mode/dlr,overground,tube,tflrail/Disruption");

        app.request(json).then(function (response) {
          actual = response;
          done();
        });
      });

      it("Then there is a response", function () {
        assert.notEqual(actual, null);
        assert.notEqual(actual.response, null);
      });
      it("Then the session ends", function () {
        assert.equal(actual.response.shouldEndSession, true);
      });
      it("Then the speech is correct", function () {
        assert.notEqual(actual.response.outputSpeech, null);
        assert.equal(actual.response.outputSpeech.type, "SSML");
        assert.equal(actual.response.outputSpeech.ssml, "<speak>Sorry, something went wrong.</speak>");
      });
    });
  });

  describe("When the status intent is requested", function () {

    var json;

    beforeEach(function () {

      tflApi.appId = "MyApplicationId";
      tflApi.appKey = "MyApplicationKey";

      json = helpers.intentRequest("StatusIntent", {
        "LINE": {
          "value": "waterloo & city",
          "name": "LINE"
        }
      });
    });

    describe("Given there is a good service", function () {

      var actual;

      beforeEach(function (done) {

        mock.tfl.success(
          "/Line/waterloo-city/Status",
          [
            {
              "id": "waterloo-city",
              "name": "Waterloo & City",
              "lineStatuses": [
                {
                  "statusSeverity": 10
                }
              ]
            }
          ]);

        app.request(json).then(function (response) {
          actual = response;
          done();
        });
      });

      it("Then there is a response", function () {
        assert.notEqual(actual, null);
        assert.notEqual(actual.response, null);
      });
      it("Then the session ends", function () {
        assert.equal(actual.response.shouldEndSession, true);
      });
      it("Then the speech is correct", function () {
        assert.notEqual(actual.response.outputSpeech, null);
        assert.equal(actual.response.outputSpeech.type, "SSML");
        assert.equal(actual.response.outputSpeech.ssml, "<speak>There is a good service on the Waterloo &amp; City line.</speak>");
      });
      it("Then the card is correct", function () {
        assert.notEqual(actual.response.card, null);
        assert.equal(actual.response.card.type, "Standard");
        assert.equal(actual.response.card.title, "Waterloo & City Line Status");
        assert.equal(actual.response.card.text, "There is a good service on the Waterloo &amp; City line.");
      });
    });

    describe("Given there is disruption", function () {

      var actual;

      beforeEach(function (done) {

        mock.tfl.success(
          "/Line/waterloo-city/Status",
          [
            {
              "id": "waterloo-city",
              "name": "Waterloo & City",
              "lineStatuses": [
                {
                  "statusSeverity": 6,
                  "reason": "Waterloo & City Line: SEVERE DELAYS due to a person ill on a train earlier at Waterloo.",
                  "disruption": {
                    "description": "Waterloo & City Line: SEVERE DELAYS due to a person ill on a train earlier at Waterloo."
                  }
                }
              ]
            }
          ]);

        app.request(json).then(function (response) {
          actual = response;
          done();
        });
      });

      it("Then there is a response", function () {
        assert.notEqual(actual, null);
        assert.notEqual(actual.response, null);
      });
      it("Then the session ends", function () {
        assert.equal(actual.response.shouldEndSession, true);
      });
      it("Then the speech is correct", function () {
        assert.notEqual(actual.response.outputSpeech, null);
        assert.equal(actual.response.outputSpeech.type, "SSML");
        assert.equal(actual.response.outputSpeech.ssml, "<speak>SEVERE DELAYS due to a person ill on a train earlier at Waterloo.</speak>");
      });
      it("Then the card is correct", function () {
        assert.notEqual(actual.response.card, null);
        assert.equal(actual.response.card.type, "Standard");
        assert.equal(actual.response.card.title, "Waterloo & City Line Status");
        assert.equal(actual.response.card.text, "SEVERE DELAYS due to a person ill on a train earlier at Waterloo.");
      });
    });

    describe("Given an error occurs", function () {

      var actual;

      beforeEach(function (done) {

        mock.tfl.failure("/Line/waterloo-city/Status");

        app.request(json).then(function (response) {
          actual = response;
          done();
        });
      });

      it("Then there is a response", function () {
        assert.notEqual(actual, null);
        assert.notEqual(actual.response, null);
      });
      it("Then the session ends", function () {
        assert.equal(actual.response.shouldEndSession, true);
      });
      it("Then the speech is correct", function () {
        assert.notEqual(actual.response.outputSpeech, null);
        assert.equal(actual.response.outputSpeech.type, "SSML");
        assert.equal(actual.response.outputSpeech.ssml, "<speak>Sorry, something went wrong.</speak>");
      });
    });
  });

  describe("When help is requested", function () {

    var actual;

    beforeEach(function (done) {

      var json = helpers.intentRequest("AMAZON.HelpIntent");

      app.request(json).then(function (response) {
        actual = response;
        done();
      });
    });

    it("Then there is a response", function () {
      assert.notEqual(actual, null);
      assert.notEqual(actual.response, null);
    });
    it("Then the session does not end", function () {
      assert.equal(actual.response.shouldEndSession, false);
    });
    it("Then the speech is correct", function () {
      assert.notEqual(actual.response.outputSpeech, null);
      assert.equal(actual.response.outputSpeech.type, "SSML");
      assert.equal(actual.response.outputSpeech.ssml, "<speak><p>This skill allows you to check for the status of a specific line, or for disruption in general. You can ask about any London Underground line, London Overground, the Docklands Light Railway or TfL Rail.</p> <p>Asking about disruption in general provides information about any lines that are currently experiencing issues, such as any delays or planned closures.</p> <p>Asking for the status for a specific line provides a summary of the current service, such as whether there is a good service or if there are any delays.</p> <p>If you link your account and setup your preferences in the London Travel website, you can ask about your commute to quickly find out the status of the lines you frequently use.</p></speak>");
    });
  });

  describe("When cancellation is requested", function () {

    var actual;

    beforeEach(function (done) {

      var json = helpers.intentRequest("AMAZON.CancelIntent");

      app.request(json).then(function (response) {
        actual = response;
        done();
      });
    });

    it("Then there is a response", function () {
      assert.notEqual(actual, null);
      assert.notEqual(actual.response, null);
    });
    it("Then the session ends", function () {
      assert.equal(actual.response.shouldEndSession, true);
    });
    it("Then there is no speech response", function () {
      assert.equal(actual.response.outputSpeech, null);
    });
  });

  describe("When stop is requested", function () {

    var actual;

    beforeEach(function (done) {

      var json = helpers.intentRequest("AMAZON.StopIntent");

      app.request(json).then(function (response) {
        actual = response;
        done();
      });
    });

    it("Then there is a response", function () {
      assert.notEqual(actual, null);
      assert.notEqual(actual.response, null);
    });
    it("Then the session ends", function () {
      assert.equal(actual.response.shouldEndSession, true);
    });
    it("Then there is no speech response", function () {
      assert.equal(actual.response.outputSpeech, null);
    });
  });

  describe("When the session is ended", function () {

    var actual;

    beforeEach(function (done) {

      var json = helpers.sessionEndRequest();

      app.request(json).then(function (response) {
        actual = response;
        done();
      });
    });

    it("Then there is a response", function () {
      assert.notEqual(actual, null);
      assert.notEqual(actual.response, null);
    });
    it("Then the session ends", function () {
      assert.equal(actual.response.shouldEndSession, true);
    });
    it("Then the speech is correct", function () {
      assert.notEqual(actual.response.outputSpeech, null);
      assert.equal(actual.response.outputSpeech.type, "SSML");
      assert.equal(actual.response.outputSpeech.ssml, "<speak>Goodbye.</speak>");
    });
  });

  describe("When the application Id is incorrect.", function () {

    var actual;

    beforeEach(function (done) {

      var json = helpers.sessionEndRequest();

      json.context.System.application.applicationId = "not my application id";
      json.session.application.applicationId = "not my application id";

      app.request(json).then(function (response) {
        actual = response;
        done();
      });
    });

    it("Then there is a response", function () {
      assert.notEqual(actual, null);
      assert.notEqual(actual.response, null);
    });
    it("Then the session ends", function () {
      assert.equal(actual.response.shouldEndSession, true);
    });
    it("Then there is no speech response", function () {
      assert.equal(actual.response.outputSpeech, null);
    });
  });

  describe("When the application Id is not set.", function () {

    var actual;

    beforeEach(function (done) {

      var json = helpers.sessionEndRequest();

      json.context.System.application.applicationId = "not my application id";
      json.session.application.applicationId = "not my application id";

      delete process.env.SKILL_ID;

      app.request(json).then(function (response) {
        actual = response;
        done();
      });
    });

    it("Then there is a response", function () {
      assert.notEqual(actual, null);
      assert.notEqual(actual.response, null);
    });
    it("Then the session ends", function () {
      assert.equal(actual.response.shouldEndSession, true);
    });
    it("Then there is no speech response", function () {
      assert.equal(actual.response.outputSpeech, null);
    });
  });
});
