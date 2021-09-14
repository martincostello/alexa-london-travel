// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Refit;

namespace MartinCostello.LondonTravel.Skill.Clients;

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
    Task<IList<ServiceDisruption>> GetDisruptionAsync(
        string modes,
        [AliasAs("app_id")] string applicationId,
        [AliasAs("app_key")] string applicationKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the statuses for the specified line Ids as an asynchronous operation.
    /// </summary>
    /// <param name="ids">A comma-separated list of the line Ids to get the status for.</param>
    /// <param name="applicationId">The application Id to use.</param>
    /// <param name="applicationKey">The application key to use.</param>
    /// <param name="cancellationToken">The optional cancellation token to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the
    /// statuses of for the line Ids specified by <paramref name="ids"/>.
    /// </returns>
    [Get("/Line/{ids}/Status")]
    Task<IList<Line>> GetLineStatusAsync(
        string ids,
        [AliasAs("app_id")] string applicationId,
        [AliasAs("app_key")] string applicationKey,
        CancellationToken cancellationToken = default);
}
