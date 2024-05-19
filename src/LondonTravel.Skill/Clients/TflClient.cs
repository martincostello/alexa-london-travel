// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Skill.Clients;

/// <summary>
/// Defines an HTTP client for the TfL API.
/// </summary>
internal sealed partial class TflClient(HttpClient httpClient, ILogger<TflClient> logger)
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
    public async Task<IList<ServiceDisruption>> GetDisruptionAsync(
        string modes,
        string applicationId,
        string applicationKey,
        CancellationToken cancellationToken = default)
    {
        string requestUrl = GetRequestUrl($"/Line/Mode/{Uri.EscapeDataString(modes)}/Disruption", applicationId, applicationKey);

        try
        {
            return await httpClient.GetFromJsonAsync(requestUrl, AppJsonSerializerContext.Default.IListServiceDisruption, cancellationToken) ?? [];
        }
        catch (Exception ex)
        {
            Log.GetDisruptionFailed(logger, ex, modes);
            throw;
        }
    }

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
    public async Task<IList<Line>> GetLineStatusAsync(
        string ids,
        string applicationId,
        string applicationKey,
        CancellationToken cancellationToken = default)
    {
        string requestUrl = GetRequestUrl($"/Line/{Uri.EscapeDataString(ids)}/Status", applicationId, applicationKey);

        try
        {
            return await httpClient.GetFromJsonAsync(requestUrl, AppJsonSerializerContext.Default.IListLine, cancellationToken) ?? [];
        }
        catch (Exception ex)
        {
            Log.GetLineStatusFailed(logger, ex, ids);
            throw;
        }
    }

    private static string GetRequestUrl(
        string uri,
        string applicationId,
        string applicationKey)
    {
        KeyValuePair<string, string?>[] parameters =
        [
            KeyValuePair.Create<string, string?>("app_id", applicationId),
            KeyValuePair.Create<string, string?>("app_key", applicationKey),
        ];

        return QueryHelpers.AddQueryString(uri, parameters);
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Warning,
            Message = "Failed to get disruption for modes {Modes}.")]
        public static partial void GetDisruptionFailed(
            ILogger logger,
            Exception exception,
            string modes);

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Warning,
            Message = "Failed to get line status for line Ids {Ids}.")]
        public static partial void GetLineStatusFailed(
            ILogger logger,
            Exception exception,
            string ids);
    }
}
