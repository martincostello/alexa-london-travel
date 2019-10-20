// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Refit;

namespace MartinCostello.LondonTravel.Skill.Clients
{
    /// <summary>
    /// A class representing an implementation of <see cref="IContentSerializer"/>
    /// for the new <c>System.Text.Json</c> serializer. This class cannot be inherited.
    /// </summary>
    public sealed class SystemTextJsonContentSerializer : IContentSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemTextJsonContentSerializer"/> class.
        /// </summary>
        /// <param name="options">The <see cref="JsonOptions"/> to use.</param>
        public SystemTextJsonContentSerializer(JsonSerializerOptions options)
        {
            SerializerOptions = options;
        }

        /// <summary>
        /// Gets the <see cref="JsonSerializerOptions"/> to use.
        /// </summary>
        private JsonSerializerOptions SerializerOptions { get; }

        /// <inheritdoc/>
        public async Task<T> DeserializeAsync<T>(HttpContent content)
        {
            using (Stream utf8Json = await content.ReadAsStreamAsync())
            {
                return await JsonSerializer.DeserializeAsync<T>(utf8Json, SerializerOptions);
            }
        }

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public Task<HttpContent> SerializeAsync<T>(T item)
        {
            throw new NotImplementedException();
        }
    }
}
