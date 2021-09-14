// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Alexa.NET.Request;
using Alexa.NET.Response;

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// Defines an intent responder.
/// </summary>
public interface IIntent
{
    /// <summary>
    /// Generates the response for the intent as an asynchronous operation.
    /// </summary>
    /// <param name="intent">The intent to respond to.</param>
    /// <param name="session">The Alexa session.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation
    /// to get the response for the intent for the current Alexa session.
    /// </returns>
    Task<SkillResponse> RespondAsync(Intent intent, Session session);
}
