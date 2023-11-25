// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request.Type;

namespace MartinCostello.LondonTravel.Skill;

public static class SkillRequestExtensions
{
    public static Alexa.NET.Request.SkillRequest FromModel(this Models.SkillRequest request)
    {
        var result = new Alexa.NET.Request.SkillRequest()
        {
            Context = new()
            {
                System = new()
                {
                    ApiAccessToken = request.Context.System.ApiAccessToken,
                    ApiEndpoint = request.Context.System.ApiEndpoint,
                    Application = new()
                    {
                        ApplicationId = request.Context.System.Application.ApplicationId,
                    },
                    Device = new()
                    {
                        DeviceID = request.Context.System.Device.DeviceId,
                        PersistentEndpointID = request.Context.System.Device.PersistentEndpointId,
                        SupportedInterfaces = request.Context.System.Device.SupportedInterfaces,
                    },
                    User = new()
                    {
                        AccessToken = request.Context.System.User.AccessToken,
                        Permissions = new(),
                        UserId = request.Context.System.User.UserId,
                    },
                },
            },
            Session = new()
            {
                Application = new()
                {
                    ApplicationId = request.Session.Application.ApplicationId,
                },
                Attributes = request.Session.Attributes,
                New = request.Session.New,
                SessionId = request.Session.SessionId,
                User = new()
                {
                    AccessToken = request.Session.User.AccessToken,
                    Permissions = new(),
                    UserId = request.Session.User.UserId,
                },
            },
            Version = request.Version,
        };

        result.Request = request.Request switch
        {
            Models.IntentRequest intent => new IntentRequest()
            {
                DialogState = intent.DialogState,
                Intent = new()
                {
                    Name = intent.Intent.Name,
                    Slots = intent.Intent.Slots?.ToDictionary((p) => p.Key, (p) => new Alexa.NET.Request.Slot()
                    {
                        Name = p.Value.Name,
                        Value = p.Value.Value,
                    }),
                },
            },
            Models.LaunchRequest launch => new LaunchRequest(),
            Models.SessionEndedRequest ended => new SessionEndedRequest()
            {
                Reason = Enum.Parse<Reason>(ended.Reason.ToString()),
                Error = ended.Error is null ? null : new()
                {
                    Message = ended.Error.Message,
                    Type = Enum.Parse<ErrorType>(ended.Error.Type.ToString()),
                },
            },
            Models.SystemExceptionRequest exception => new SystemExceptionRequest()
            {
                ErrorCause = new()
                {
                    requestId = exception.ErrorCause.RequestId,
                },
                Error = new()
                {
                    Message = exception.Error.Message,
                    Type = Enum.Parse<ErrorType>(exception.Error.Type.ToString()),
                },
            },
            _ => new UnknownRequest(),
        };

        result.Request.Locale = request.Request.Locale;
        result.Request.RequestId = request.Request.RequestId;
        result.Request.Timestamp = request.Request.Timestamp;
        result.Request.Type = request.Request.Type;

        return result;
    }

    private sealed class UnknownRequest : Request
    {
    }
}
