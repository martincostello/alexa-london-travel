// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Shouldly;
using Xunit.Abstractions;

namespace MartinCostello.LondonTravel.Skill
{
    public abstract class FunctionTests
    {
        protected FunctionTests(ITestOutputHelper outputHelper)
        {
            Logger = new XunitLambdaLogger(outputHelper);
        }

        protected ILambdaLogger Logger { get; }

        protected virtual ResponseBody AssertResponse(SkillResponse actual, bool? shouldEndSession = true)
        {
            actual.ShouldNotBeNull();
            actual.Version.ShouldBe("1.0");
            actual.Response.ShouldNotBeNull();
            actual.Response.ShouldEndSession.ShouldBe(shouldEndSession);

            return actual.Response;
        }

        protected virtual ILambdaContext CreateContext()
        {
            return new TestLambdaContext()
            {
                Logger = Logger,
            };
        }

        protected virtual AlexaFunction CreateFunction()
        {
            var config = new SkillConfiguration();
            return new AlexaFunction(config);
        }

        protected virtual SkillRequest CreateIntentRequest(string name)
        {
            var request = new IntentRequest()
            {
                Intent = new Intent()
                {
                    Name = name,
                },
            };

            return CreateRequest(request);
        }

        protected virtual SkillRequest CreateRequest<T>(T request = null)
            where T : Request, new()
        {
            return new SkillRequest()
            {
                Request = request ?? new T(),
                Session = new Session(),
            };
        }
    }
}
