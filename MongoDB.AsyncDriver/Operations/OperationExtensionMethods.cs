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
    public static class OperationExtensionMethods
    {
        // methods
        public static TResult Execute<TResult>(this IWriteOperation<TResult> operation, ICluster cluster, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            return operation.ExecuteAsync(cluster, timeout, cancellationToken).GetAwaiter().GetResult();
        }

        public static TResult Execute<TResult>(this IWriteOperation<TResult> operation, IWritableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            return operation.ExecuteAsync(connection, timeout, cancellationToken).GetAwaiter().GetResult();
        }

        public static TResult Execute<TResult>(this IWriteOperation<TResult> operation, IWritableNode node, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            return operation.ExecuteAsync(node, timeout, cancellationToken).GetAwaiter().GetResult();
        }

        public static TResult Execute<TResult>(this IReadOperation<TResult> operation, ICluster cluster, ReadPreference readPreference, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            return operation.ExecuteAsync(cluster, readPreference, timeout, cancellationToken).GetAwaiter().GetResult();
        }

        public static async Task<TResult> ExecuteAsync<TResult>(this IWriteOperation<TResult> operation, ICluster cluster, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var slidingTimeout = new SlidingTimeout(timeout);
            var node = await cluster.GetWritableNodeAsync(slidingTimeout, cancellationToken);
            using (var connection = await node.GetConnectionAsync(slidingTimeout, cancellationToken))
            {
                return await operation.ExecuteAsync(connection, slidingTimeout, cancellationToken);
            }
        }

        public static async Task<TResult> ExecuteAsync<TResult>(this IWriteOperation<TResult> operation, IWritableNode node, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var slidingTimeout = new SlidingTimeout(timeout);
            using (var connection = await node.GetConnectionAsync(slidingTimeout, cancellationToken))
            {
                return await operation.ExecuteAsync(connection, slidingTimeout, cancellationToken);
            }
        }

        public static async Task<TResult> ExecuteAsync<TResult>(this IReadOperation<TResult> operation, ICluster cluster, ReadPreference readPreference, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var slidingTimeout = new SlidingTimeout(timeout);
            var node = await cluster.GetReadableNodeAsync(readPreference, slidingTimeout, cancellationToken);
            using (var connection = await node.GetConnectionAsync(slidingTimeout, cancellationToken))
            {
                return await operation.ExecuteAsync(connection, slidingTimeout, cancellationToken);
            }
        }
    }
}
