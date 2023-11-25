// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request.Type;
using SkillRequest = Alexa.NET.Request.SkillRequest;

namespace MartinCostello.LondonTravel.Skill;

public static class SkillRequestExtensions
{
    public static SkillRequest FromModel(this Models.SkillRequest request)
    {
        var result = MapRequest(request);

        result.Request = request.Request switch
        {
            Models.IntentRequest intent => MapIntent(intent),
            Models.LaunchRequest => new LaunchRequest(),
            Models.SessionEndedRequest session => MapSessionEnd(session),
            Models.SystemExceptionRequest exception => MapException(exception),
            _ => new UnknownRequest(),
        };

        result.Request.Locale = request.Request.Locale;
        result.Request.RequestId = request.Request.RequestId;
        result.Request.Timestamp = request.Request.Timestamp;
        result.Request.Type = request.Request.Type;

        return result;
    }

    private static SkillRequest MapRequest(Models.SkillRequest request)
    {
        var result = new SkillRequest() { Version = request.Version };

        if (request.Context is { } context)
        {
            result.Context = new();

            if (context.System is { } system)
            {
                result.Context.System = new()
                {
                    ApiAccessToken = system.ApiAccessToken,
                    ApiEndpoint = system.ApiEndpoint,
                };

                if (system.Application is { } application)
                {
                    result.Context.System.Application = new()
                    {
                        ApplicationId = application.ApplicationId,
                    };
                }

                if (system.Device is { } device)
                {
                    result.Context.System.Device = new()
                    {
                        DeviceID = device.DeviceId,
                        PersistentEndpointID = device.PersistentEndpointId,
                        SupportedInterfaces = device.SupportedInterfaces,
                    };
                }

                if (system.User is { } user)
                {
                    result.Context.System.User = new()
                    {
                        AccessToken = user.AccessToken,
                        Permissions = new(),
                        UserId = user.UserId,
                    };
                }
            }
        }

        if (request.Session is { } session)
        {
            result.Session = new()
            {
                Attributes = session.Attributes,
                New = session.New,
                SessionId = session.SessionId,
            };

            if (session.Application is { } application)
            {
                result.Session.Application = new()
                {
                    ApplicationId = application.ApplicationId,
                };
            }

            if (session.User is { } user)
            {
                result.Session.User = new()
                {
                    AccessToken = user.AccessToken,
                    Permissions = new(),
                    UserId = user.UserId,
                };
            }
        }

        return result;
    }

    private static SystemExceptionRequest MapException(Models.SystemExceptionRequest exception)
    {
        var result = new SystemExceptionRequest();

        if (exception.ErrorCause is { } cause)
        {
            result.ErrorCause = new() { requestId = cause.RequestId };
        }

        if (exception.Error is { } error)
        {
            result.Error = new()
            {
                Message = error.Message,
                Type = Enum.Parse<ErrorType>(error.Type.ToString()),
            };
        }

        return result;
    }

    private static IntentRequest MapIntent(Models.IntentRequest request)
    {
        var result = new IntentRequest()
        {
            DialogState = request.DialogState,
        };

        if (request.Intent is { } intent)
        {
            result.Intent = new() { Name = intent.Name };

            if (intent.Slots is { } slots)
            {
                result.Intent.Slots = slots.Where((p) => p.Value is not null).ToDictionary((p) => p.Key, (p) => new Alexa.NET.Request.Slot()
                {
                    Name = p.Value.Name,
                    Value = p.Value.Value,
                });
            }
        }

        return result;
    }

    private static SessionEndedRequest MapSessionEnd(Models.SessionEndedRequest session)
    {
        var result = new SessionEndedRequest()
        {
            Reason = Enum.Parse<Reason>(session.Reason.ToString()),
        };

        if (session.Error is { } error)
        {
            result.Error = new()
            {
                Message = error.Message,
                Type = Enum.Parse<ErrorType>(error.Type.ToString()),
            };
        }

        return result;
    }

    private sealed class UnknownRequest : Request
    {
    }
}
