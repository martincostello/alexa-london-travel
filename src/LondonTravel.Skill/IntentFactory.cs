// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Skill.Intents;
using MartinCostello.LondonTravel.Skill.Models;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class representing a factory for <see cref="IIntent"/> instances. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IntentFactory"/> class.
/// </remarks>
/// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
internal sealed class IntentFactory(IServiceProvider serviceProvider)
{
    /// <summary>
    /// Creates an intent responder for the specified intent.
    /// </summary>
    /// <param name="intent">The intent to create a responder for.</param>
    /// <returns>
    /// The <see cref="IIntent"/> to use to generate a response for the intent.
    /// </returns>
    public IIntent Create(Intent intent)
    {
        return intent.Name switch
        {
            "AMAZON.CancelIntent" or "AMAZON.StopIntent" => serviceProvider.GetRequiredService<EmptyIntent>(),
            "AMAZON.HelpIntent" => serviceProvider.GetRequiredService<HelpIntent>(),
            "CommuteIntent" => serviceProvider.GetRequiredService<CommuteIntent>(),
            "DisruptionIntent" => serviceProvider.GetRequiredService<DisruptionIntent>(),
            "StatusIntent" => serviceProvider.GetRequiredService<StatusIntent>(),
            _ => serviceProvider.GetRequiredService<UnknownIntent>(),
        };
    }
}
