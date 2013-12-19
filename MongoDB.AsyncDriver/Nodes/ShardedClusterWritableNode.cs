/* Copyright 2013-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents a writable ShardedClusterNode.
    /// </summary>
    internal class ShardedClusterWritableNode : ShardedClusterReadableNode, IWritableNode
    {
        // fields
        private readonly IWritableNode _wrapped;

        // constructors
        public ShardedClusterWritableNode(IWritableNode wrapped)
            : base(wrapped, ReadPreference.Primary)
        {
            _wrapped = wrapped;
        }

        // methods
        public new Task<IWritableConnection> GetConnectionAsync(TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            return _wrapped.GetConnectionAsync(timeout, cancellationToken);
        }

        public new Task<IWritableConnection> GetDedicatedConnectionAsync(TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            return _wrapped.GetDedicatedConnectionAsync(timeout, cancellationToken);
        }
    }
}
