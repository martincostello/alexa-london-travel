// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Refit;

namespace MartinCostello.LondonTravel.Skill.Clients
{
    /// <summary>
    /// Defines an HTTP client for the TfL API.
    /// </summary>
    internal interface ITflClient
    {
        /// <summary>
        /// Gets the disruption for the specified modes of travel as an asynchronous operation.
        /// </summary>
        /// <param name="modes">A comma-separated list of modes of travel.</param>
        /// <param name="applicationId">The application Id to use.</param>
        /// <param name="applicationKey">The application key to use.</param>
        /// <param name="cancellationToken">The optional cancellation token to use.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the
        /// disruption for the modes specified by <paramref name="modes"/>.
        /// </returns>
        [Get("/Line/Mode/{modes}/Disruption")]
        Task<ICollection<ServiceDisruption>> GetDisruptionAsync(
            string modes,
            [AliasAs("app_id")] string applicationId,
            [AliasAs("app_key")] string applicationKey,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the status for the specified line Id as an asynchronous operation.
        /// </summary>
        /// <param name="id">The Id of the line to get disruption for.</param>
        /// <param name="applicationId">The application Id to use.</param>
        /// <param name="applicationKey">The application key to use.</param>
        /// <param name="cancellationToken">The optional cancellation token to use.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the
        /// disruption for the line associated with the Id specified by <paramref name="id"/>.
        /// </returns>
        [Get("/Line/{id}/Status")]
        Task<HttpResponseMessage> GetLineStatusAsync(
            string id,
            [AliasAs("app_id")] string applicationId,
            [AliasAs("app_key")] string applicationKey,
            CancellationToken cancellationToken = default);
    }
}
