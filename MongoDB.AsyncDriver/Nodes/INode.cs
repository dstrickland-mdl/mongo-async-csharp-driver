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
    /// Represents a MongoDB node.
    /// </summary>
    public interface INode : IDisposable
    {
        // properties
        ICluster Cluster { get; }
        DnsEndPoint EndPoint { get; }
        NodeInfo Info { get; }
        bool IsReadOnly { get; }

        // methods
        Task<NodeInfo> GetInfoAsync(int minimumRevision, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// Represents a MongoDB node that can be used for read operations.
    /// </summary>
    public interface IReadableNode : INode
    {
        Task<IReadableConnection> GetConnectionAsync(TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadableConnection> GetDedicatedConnectionAsync(TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// Represents a MongoDB node that can be used for write (and read) operations.
    /// </summary>
    public interface IWritableNode : IReadableNode
    {
        // methods
        new Task<IWritableConnection> GetConnectionAsync(TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken));
        new Task<IWritableConnection> GetDedicatedConnectionAsync(TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// Represents a node that hasn't been wrapped.
    /// </summary>
    public interface IRootNode : IWritableNode
    {
    }
}
