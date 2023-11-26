// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class representing the implementation of the London Travel Alexa skill. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AlexaSkill"/> class.
/// </remarks>
/// <param name="intentFactory">The factory to use for the skill intents.</param>
/// <param name="logger">The logger to use.</param>
internal sealed class AlexaSkill(
    IntentFactory intentFactory,
    ILogger<AlexaSkill> logger)
{
    /// <summary>
    /// Handles a system error.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <param name="session">The Alexa session.</param>
    /// <returns>
    /// The <see cref="ResponseBody"/> to return from the skill.
    /// </returns>
    public SkillResponse OnError(SystemExceptionRequest error, Session session)
    {
        Log.SystemError(
            logger,
            session.SessionId,
            error.Error.Type.ToString(),
            error.ErrorCause?.requestId,
            error.Error.Message);

        return SkillResponseBuilder
            .Tell(Strings.InternalError)
            .Build();
    }

    /// <summary>
    /// Handles an error.
    /// </summary>
    /// <param name="exception">The exception that occurred, if any.</param>
    /// <param name="session">The Alexa session.</param>
    /// <returns>
    /// The <see cref="ResponseBody"/> to return from the skill.
    /// </returns>
    public SkillResponse OnError(Exception exception, Session session)
    {
        Log.HandlerException(logger, exception, session.SessionId);

        return SkillResponseBuilder
            .Tell(Strings.InternalError)
            .Build();
    }

    /// <summary>
    /// Handles the skill intent as an asynchronous operation.
    /// </summary>
    /// <param name="intent">The intent to handle.</param>
    /// <param name="session">The Alexa session.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation
    /// which returns the <see cref="ResponseBody"/> to return from the skill.
    /// </returns>
    public async Task<SkillResponse> OnIntentAsync(Intent intent, Session session)
    {
        IIntent userIntent = intentFactory.Create(intent);

        return await userIntent.RespondAsync(intent, session);
    }

    /// <summary>
    /// Handles the skill being launched.
    /// </summary>
    /// <param name="session">The Alexa session.</param>
    /// <returns>
    /// The <see cref="ResponseBody"/> to return from the skill.
    /// </returns>
    public SkillResponse OnLaunch(Session session)
    {
        Log.SessionLaunched(logger, session.User.UserId, session.SessionId);

        return SkillResponseBuilder
            .Tell(Strings.LaunchResponse)
            .ShouldNotEndSession()
            .Build();
    }

    /// <summary>
    /// Handles the skill session ending.
    /// </summary>
    /// <param name="session">The Alexa session.</param>
    /// <returns>
    /// The <see cref="ResponseBody"/> to return from the skill.
    /// </returns>
    public SkillResponse OnSessionEnded(Session session)
    {
        Log.SessionEnded(logger, session.User.UserId, session.SessionId);

        return SkillResponseBuilder
            .Tell(Strings.SessionEndResponse)
            .Build();
    }
}
