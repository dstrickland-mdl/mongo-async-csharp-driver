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
    /// Represents a pooled connection.
    /// </summary>
    internal abstract class PooledConnection : IConnectionInternal
    {
        // fields
        private DateTime _createdAt;
        private bool _disposed;
        private DateTime _lastUsedAt;
        private readonly IRootConnection _wrapped;

        // constructors
        public PooledConnection(IRootConnection wrapped)
        {
            _wrapped = wrapped;
            _createdAt = DateTime.UtcNow;
            _lastUsedAt = _createdAt;
        }

        // properties
        public int ConnectionId
        {
            get
            {
                ThrowIfDisposed();
                return _wrapped.ConnectionId;
            }
        }

        public DateTime CreatedAt
        {
            get
            {
                ThrowIfDisposed();
                return _createdAt;
            }
        }

        public DateTime LastUsedAt
        {
            get
            {
                ThrowIfDisposed();
                return _lastUsedAt;
            }
        }

        public INode Node
        {
            get
            {
                ThrowIfDisposed();
                return _wrapped.Node;
            }
        }

        internal IRootConnection Wrapped
        {
            get { return _wrapped; }
        }

        // methods
        public void Dispose()
        {
            if (!_disposed)
            {
                _wrapped.Dispose();
                _disposed = true;
            }
        }

        public abstract IConnection Fork();

        public Task<ReplyMessage<TDocument>> ReceiveMessageAsync<TDocument>(int responseTo, IBsonSerializer<TDocument> serializer, TimeSpan timeout, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            _lastUsedAt = DateTime.UtcNow;
            return _wrapped.ReceiveMessageAsync<TDocument>(responseTo, serializer, timeout, cancellationToken);
        }

        public Task SendMessagesAsync(IEnumerable<RequestMessage> messages, TimeSpan timeout, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            _lastUsedAt = DateTime.UtcNow;
            return _wrapped.SendMessagesAsync(messages, timeout, cancellationToken);
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed) { throw new ObjectDisposedException(this.GetType().Name); }
        }
    }
}
