// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Refit;

namespace MartinCostello.LondonTravel.Skill.Clients
{
    /// <summary>
    /// Defines the client for the skill's API.
    /// </summary>
    public interface ISkillClient
    {
        /// <summary>
        /// Get the current user's preferences from the skill's API as an asynchronous operation.
        /// </summary>
        /// <param name="cancellationToken">The optional cancellation token to use.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the current user's preferences from the skill API.
        /// </returns>
        [Get("/api/preferences")]
        Task<HttpResponseMessage> GetDisruptionAsync(CancellationToken cancellationToken = default);
    }
}
