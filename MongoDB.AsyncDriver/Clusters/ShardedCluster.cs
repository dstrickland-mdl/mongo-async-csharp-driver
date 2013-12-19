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
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents a sharded cluster (accessed through one or more shard router nodes).
    /// </summary>
    public class ShardedCluster : ICluster
    {
        // events
        public event EventHandler InfoChanged;

        // properties
        public ClusterInfo Info
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyList<INode> Nodes
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyList<IReadableNode> ReadableNodes
        {
            get { throw new NotImplementedException(); }
        }

        public ClusterSettings Settings
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyList<IWritableNode> WritableNodes
        {
            get { throw new NotImplementedException(); }
        }

        // methods
        private Node ChooseNode()
        {
            throw new NotImplementedException();
        }

        private async Task<Node> ChooseNodeAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            var slidingTimeout = new SlidingTimeout(timeout);
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var revision = Info.Revision;
                var node = ChooseNode();
                if (node != null)
                {
                    return node;
                }
                await GetInfoAsync(revision + 1, slidingTimeout, cancellationToken);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<ClusterInfo> GetInfoAsync(int minimumRevision = 0, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public async Task<IReadableNode> GetReadableNodeAsync(ReadPreference readPreference, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var node = await ChooseNodeAsync(timeout, cancellationToken);
            return new ShardedClusterReadableNode(node, readPreference);
        }

        public async Task<IWritableNode> GetWritableNodeAsync(TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var node = await ChooseNodeAsync(timeout, cancellationToken);
            return new ShardedClusterWritableNode(node);
        }

        private void OnInfoChanged()
        {
            var handler = InfoChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
