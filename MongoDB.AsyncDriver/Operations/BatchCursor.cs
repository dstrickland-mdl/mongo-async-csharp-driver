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
    public class BatchCursor<TDocument> : IAsyncEnumerator<List<TDocument>>
    {
        // fields
        private readonly int _batchSize;
        private readonly CancellationToken _cancellationToken;
        private readonly string _collectionName;
        private readonly IConnection _connection;
        private List<TDocument> _currentBatch;
        private long _cursorId;
        private readonly string _databaseName;
        private bool _disposed;
        private List<TDocument> _firstBatch;
        private readonly BsonDocument _query;
        private readonly IBsonSerializer<TDocument> _serializer;
        private readonly SlidingTimeout _slidingTimeout;

        // constructors
        public BatchCursor(IConnection connection, string databaseName, string collectionName, BsonDocument query, List<TDocument> firstBatch, long cursorId, int batchSize, IBsonSerializer<TDocument> serializer, SlidingTimeout slidingTimeout, CancellationToken cancellationToken)
        {
            if (connection == null) { throw new ArgumentNullException("connection"); }
            if (databaseName == null) { throw new ArgumentNullException("databaseName"); }
            if (collectionName == null) { throw new ArgumentNullException("collectionName"); }
            if (firstBatch == null) { throw new ArgumentNullException("firstBatch"); }
            if (serializer == null) { throw new ArgumentNullException("serializer"); }

            _connection = (cursorId != 0) ? connection.Fork() : null;
            _databaseName = databaseName;
            _collectionName = collectionName;
            _query = query;
            _firstBatch = firstBatch;
            _cursorId = cursorId;
            _batchSize = batchSize;
            _serializer = serializer;
            _slidingTimeout = slidingTimeout;
            _cancellationToken = cancellationToken;
        }

        // properties
        public List<TDocument> Current
        {
            get
            {
                return _currentBatch;
            }
        }

        // methods
        public void Dispose()
        {
            if (_disposed)
            {
                try
                {
                    if (_cursorId != 0)
                    {
                        var task = KillCursorAsync(_cursorId);
                    }
                }
                catch
                {
                    // ignore exceptions
                }
                if (_connection != null)
                {
                    _connection.Dispose();
                }
                _disposed = true;
            }
        }

        private async Task<Batch> GetNextBatchAsync(IConnection connection)
        {
            var getMoreMessage = new GetMoreMessage(
                RequestMessage.GetNextRequestId(),
                _databaseName,
                _collectionName,
                _cursorId,
                _batchSize);

            await connection.SendMessageAsync(getMoreMessage, _slidingTimeout, _cancellationToken);
            var reply = await connection.ReceiveMessageAsync<TDocument>(getMoreMessage.RequestId, _serializer, _slidingTimeout, _cancellationToken);
            if (reply.QueryFailure)
            {
                var message = string.Format("GetMore QueryFailure: {0}.", reply.QueryFailureDocument);
                throw new QueryException(message, _query, reply.QueryFailureDocument);
            }
            return new Batch { Documents = reply.Documents, CursorId = reply.CursorId };
        }

        private async Task KillCursorAsync(long cursorId)
        {
            var killCursorsMessage = new KillCursorsMessage(
                RequestMessage.GetNextRequestId(),
                new[] { cursorId });
            await _connection.SendMessageAsync(killCursorsMessage, _slidingTimeout, _cancellationToken);
        }

        public async Task<bool> MoveNextAsync()
        {
            if (_firstBatch != null)
            {
                _currentBatch = _firstBatch;
                _firstBatch = null;
                return true;
            }

            if (_currentBatch == null)
            {
                return false;
            }

            if (_cursorId == 0)
            {
                _currentBatch = null;
                return false;
            }

            var batch = await GetNextBatchAsync(_connection);
            _currentBatch = batch.Documents;
            _cursorId = batch.CursorId;
            return true;
        }

        public DocumentCursor<TDocument> ToDocumentCursor()
        {
            return new DocumentCursor<TDocument>(this);
        }

        // nested classes
        private struct Batch
        {
            public List<TDocument> Documents;
            public long CursorId;
        }
    }
}
