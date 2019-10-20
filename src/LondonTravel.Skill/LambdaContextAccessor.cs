// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Threading;
using Amazon.Lambda.Core;

namespace MartinCostello.LondonTravel.Skill
{
    /// <summary>
    /// A class representing an accessor for <see cref="ILambdaContext"/>. This class cannot be inherited.
    /// </summary>
    internal sealed class LambdaContextAccessor
    {
        /// <summary>
        /// A backing field for the <see cref="ILambdaContext"/> for the current thread.
        /// </summary>
        private static readonly AsyncLocal<ILambdaContext> _current = new AsyncLocal<ILambdaContext>();

        /// <summary>
        /// Gets or sets the current <see cref="ILambdaContext"/>.
        /// </summary>
        public ILambdaContext LambdaContext
        {
            get { return _current.Value; }
            set { _current.Value = value; }
        }
    }
}
