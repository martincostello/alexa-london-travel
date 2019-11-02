// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Skill.Integration
{
    /// <summary>
    /// A class representing a response from an AWS Lambda function. This class cannot be inherited.
    /// </summary>
    public sealed class LambdaResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaResponse"/> class.
        /// </summary>
        /// <param name="content">The raw content of the response from the Lambda function.</param>
        /// <param name="isSuccessful">Whether the response indicates the request was successfully handled.</param>
        internal LambdaResponse(byte[] content, bool isSuccessful)
        {
            Content = content;
            IsSuccessful = isSuccessful;
        }

        /// <summary>
        /// Gets the raw byte content of the response from the function.
        /// </summary>
        public byte[] Content { get; }

        /// <summary>
        /// Gets a value indicating whether the response indicates the request was successfully handled.
        /// </summary>
        public bool IsSuccessful { get; }
    }
}
