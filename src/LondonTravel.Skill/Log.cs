// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class containing log messages for the skill.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
internal static partial class Log
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Failed to handle request for session {SessionId} for request of type {RequestType}.")]
    public static partial void HandlerException(
        ILogger logger,
        Exception? exception,
        string sessionId,
        string requestType);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Failed to handle request for session {SessionId}. Error type {ErrorType} with cause {ErrorCause}: {ErrorMessage}")]
    public static partial void SystemError(
        ILogger logger,
        string sessionId,
        Models.AlexaErrorType errorType,
        string? errorCause,
        string errorMessage);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "User with Id {UserId} has not linked account.")]
    public static partial void AccountNotLinked(ILogger logger, string? userId);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Warning,
        Message = "Access token is invalid for user Id {UserId} and session Id {SessionId}.")]
    public static partial void InvalidAccessToken(ILogger logger, string userId, string sessionId);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "User with Id {UserId} has set no line preferences.")]
    public static partial void NoLinePreferences(ILogger logger, string userId);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Warning,
        Message = "Unknown intent {IntentName} cannot be handled for session Id {SessionId}.")]
    public static partial void UnknownIntent(ILogger logger, string intentName, string sessionId);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "Invoking skill request of type {RequestType}.")]
    public static partial void InvokingSkillRequest(ILogger logger, string requestType);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Debug,
        Message = "Launched session for user Id {UserId} and session Id {SessionId}.")]
    public static partial void SessionLaunched(ILogger logger, string userId, string sessionId);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Debug,
        Message = "Ended session for user Id {UserId} and session Id {SessionId}.")]
    public static partial void SessionEnded(ILogger logger, string userId, string sessionId);
}
