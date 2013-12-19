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
    /// Represents a MongoDB cluster.
    /// </summary>
    public interface ICluster : IDisposable
    {
        // events
        event EventHandler InfoChanged;

        // properties
        ClusterInfo Info { get; }
        IReadOnlyList<INode> Nodes { get; }
        IReadOnlyList<IReadableNode> ReadableNodes { get; }
        ClusterSettings Settings { get; }
        IReadOnlyList<IWritableNode> WritableNodes { get; }

        // methods
        Task<ClusterInfo> GetInfoAsync(int minimumRevision = 0, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadableNode> GetReadableNodeAsync(ReadPreference readPreference, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken));
        Task<IWritableNode> GetWritableNodeAsync(TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// Represents a MongoDB replica set.
    /// </summary>
    public interface IReplicaSet : ICluster
    {
        IWritableNode Primary { get; }
        IReadOnlyList<IReadableNode> Secondaries { get; }
    }
}
