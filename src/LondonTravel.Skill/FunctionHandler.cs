// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Skill.Models;

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class representing the function handler for the London Travel Amazon Alexa skill.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FunctionHandler"/> class.
/// </remarks>
/// <param name="skill">The <see cref="AlexaSkill"/> to use.</param>
/// <param name="config">The <see cref="SkillConfiguration"/> to use.</param>
internal sealed class FunctionHandler(AlexaSkill skill, SkillConfiguration config)
{
    /// <summary>
    /// Handles a request to the skill as an asynchronous operation.
    /// </summary>
    /// <param name="request">The skill request.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the skill's response.
    /// </returns>
    public async Task<SkillResponse> HandleAsync(SkillRequest request)
    {
        VerifySkillId(request);

        using (CultureSwitcher.UseCulture(request.Request.Locale ?? "en-GB"))
        {
            return await HandleRequestAsync(request);
        }
    }

    /// <summary>
    /// Handles the specified request as an asynchronous operation.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to handle the request.
    /// </returns>
    private async Task<SkillResponse> HandleRequestAsync(SkillRequest request)
    {
        try
        {
            return request.Request switch
            {
                IntentRequest intent => await skill.OnIntentAsync(intent, request.Session),
                LaunchRequest => skill.OnLaunch(request.Session),
                SessionEndedRequest => skill.OnSessionEnded(request.Session),
                SystemExceptionRequest exception => skill.OnError(exception, request.Session),
                _ => skill.OnError(null, request.Session, request.Request.Type),
            };
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            return skill.OnError(ex, request.Session, request.Request.Type);
        }
    }

    /// <summary>
    /// Verifies the skill Id.
    /// </summary>
    /// <param name="request">The function request.</param>
    private void VerifySkillId(SkillRequest request)
    {
        if (config.VerifySkillId)
        {
            string applicationId = request.Session.Application.ApplicationId;

            if (!string.Equals(applicationId, config.SkillId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Request application Id '{applicationId}' and configured skill Id '{config.SkillId}' mismatch.");
            }
        }
    }
}
