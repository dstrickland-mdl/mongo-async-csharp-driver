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
    /// Represents a wrapped node.
    /// </summary>
    public class WrappedNode : INode
    {
        // fields
        private bool _disposed;
        private readonly INode _wrapped;

        // constructors
        public WrappedNode(INode wrapped)
        {
            _wrapped = wrapped;
        }

        // properties
        public ICluster Cluster
        {
            get
            {
                ThrowIfDisposed();
                return _wrapped.Cluster;
            }
        }

        public DnsEndPoint EndPoint
        {
            get
            {
                ThrowIfDisposed();
                return _wrapped.EndPoint;
            }
        }

        public NodeInfo Info
        {
            get
            {
                ThrowIfDisposed();
                return _wrapped.Info;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                ThrowIfDisposed();
                return _wrapped.IsReadOnly;
            }
        }

        // methods
        public void Dispose()
        {
            // don't dispose _wrapped
            _disposed = true;
        }

        public Task<NodeInfo> GetInfoAsync(int minimumRevision, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            return _wrapped.GetInfoAsync(minimumRevision, timeout, cancellationToken);
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }
    }
}
