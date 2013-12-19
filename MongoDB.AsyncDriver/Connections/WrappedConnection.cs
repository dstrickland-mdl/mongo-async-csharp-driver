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
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents a wrapped connection.
    /// </summary>
    public abstract class WrappedConnection : IConnectionInternal
    {
        // fields
        private bool _disposed;
        protected readonly INode _node;
        protected readonly IConnection _wrapped;

        // constructors
        public WrappedConnection(IConnection wrapped, INode node)
        {
            _node = node;
            _wrapped = wrapped;
        }

        // properties
        public int ConnectionId
        {
            get { return _wrapped.ConnectionId; }
        }

        public INode Node
        {
            get { return _node; }
        }

        // methods
        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                _disposed = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            _wrapped.Dispose();
        }

        public IConnection Fork()
        {
            ThrowIfDisposed();
            return ForkImplementation();
        }

        protected abstract IConnection ForkImplementation();

        public virtual Task<ReplyMessage<TDocument>> ReceiveMessageAsync<TDocument>(int responseTo, IBsonSerializer<TDocument> serializer, TimeSpan timeout, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _wrapped.ReceiveMessageAsync(responseTo, serializer, timeout, cancellationToken);
        }

        public virtual Task SendMessagesAsync(IEnumerable<RequestMessage> messages, TimeSpan timeout, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _wrapped.SendMessagesAsync(messages, timeout, cancellationToken);
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
