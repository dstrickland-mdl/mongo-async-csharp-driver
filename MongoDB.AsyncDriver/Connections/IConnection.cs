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
    /// Represents a connection.
    /// </summary>
    public interface IConnection : IDisposable
    {
        // properties
        int ConnectionId { get; }
        INode Node { get; }
        IConnection Fork();
    }

    /// <summary>
    /// Represents a connection that can be used for read operations.
    /// </summary>
    public interface IReadableConnection : IConnection
    {
        new IReadableNode Node { get; }
        new IReadableConnection Fork();
    }

    /// <summary>
    /// Represents a connection that can be used for write (and read) operations.
    /// </summary>
    public interface IWritableConnection : IReadableConnection
    {
        new IWritableNode Node { get; }
        new IWritableConnection Fork();
    }

    /// <summary>
    /// Represents additional IConnection methods that are only used internally.
    /// </summary>
    public interface IConnectionInternal : IConnection
    {
        // methods
        Task<ReplyMessage<TDocument>> ReceiveMessageAsync<TDocument>(int responseTo, IBsonSerializer<TDocument> serializer, TimeSpan timeout, CancellationToken cancellationToken);
        Task SendMessagesAsync(IEnumerable<RequestMessage> messages, TimeSpan timeout, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents a connection that hasn't been wrapped.
    /// </summary>
    public interface IRootConnection : IConnectionInternal, IWritableConnection
    {
        Task OpenAsync(TimeSpan timeout, CancellationToken cancellationToken);
        void SetConnectionId(int connectionId);
    }
}
