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
    public class MockConnection : IRootConnection
    {
        #region static
        // static fields
        private static int __connectionId = -1;
        #endregion

        // fields
        private int _connectionId;
        private readonly IRootNode _node;
        private readonly IMockServer _server;

        // constructors
        public MockConnection(IRootNode node, IMockServer server)
        {
            _node = node;
            _server = server;
            _connectionId = Interlocked.Decrement(ref __connectionId);
        }

        // properties
        public int ConnectionId
        {
            get { return _connectionId;  }
        }

        public IWritableNode Node
        {
            get { return _node; }
        }

        INode IConnection.Node
        {
            get { return _node; }
        }

        IReadableNode IReadableConnection.Node
        {
            get { return _node; }
        }

        // methods
        public void Dispose()
        {
        }

        public IWritableConnection Fork()
        {
            return this;
        }

        IConnection IConnection.Fork()
        {
            return this;
        }

        IReadableConnection IReadableConnection.Fork()
        {
            return this;
        }

        public Task OpenAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<ReplyMessage<TDocument>> ReceiveMessageAsync<TDocument>(int responseTo, IBsonSerializer<TDocument> serializer, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return _server.GetReplyAsync<TDocument>(responseTo, serializer, timeout, cancellationToken);
        }

        public Task SendMessagesAsync(IEnumerable<RequestMessage> messages, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return _server.AcceptMessagesAsync(messages, timeout, cancellationToken);
        } 

        public void SetConnectionId(int connectionId)
        {
            _connectionId = connectionId;
        }
    }
}
