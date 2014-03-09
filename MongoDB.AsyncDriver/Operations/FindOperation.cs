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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public class FindOperation : FindOperation<BsonDocument>
    {
        // constructors
        public FindOperation(
            string databaseName,
            string collectionName,
            BsonDocument query = null)
            : base(databaseName, collectionName, BsonDocumentSerializer.Instance, query)
        {
        }
    }

    public class FindOperation<TDocument> : IReadOperation<BatchCursor<TDocument>>
    {
        // fields
        private readonly BsonDocument _additionalOptions;
        private readonly bool _awaitData = true;
        private readonly int _batchSize;
        private readonly string _collectionName;
        private readonly string _comment;
        private readonly string _databaseName;
        private readonly BsonDocument _fields;
        private readonly string _hint;
        private readonly int? _limit;
        private readonly bool _noCursorTimeout;
        private readonly bool _partialOk;
        private readonly BsonDocument _query;
        private readonly IBsonSerializer<TDocument> _serializer;
        private readonly int _skip;
        private readonly bool? _snapshot;
        private readonly BsonDocument _sort;
        private readonly bool _tailableCursor;

        // constructors
        public FindOperation(
            string databaseName,
            string collectionName,
            IBsonSerializer<TDocument> serializer,
            BsonDocument query = null)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _serializer = serializer;
            _query = query;
        }

        internal FindOperation(
            BsonDocument additionalOptions,
            bool awaitData,
            int batchSize,
            string collectionName,
            string comment,
            string databaseName,
            BsonDocument fields,
            string hint,
            int? limit,
            bool noCursorTimeout,
            bool partialOk,
            BsonDocument query,
            IBsonSerializer<TDocument> serializer,
            int skip,
            bool? snapshot,
            BsonDocument sort,
            bool tailableCursor)
        {
            _additionalOptions = additionalOptions;
            _awaitData = awaitData;
            _batchSize = batchSize;
            _collectionName = collectionName;
            _comment = comment;
            _databaseName = databaseName;
            _fields = fields;
            _hint = hint;
            _limit = limit;
            _noCursorTimeout = noCursorTimeout;
            _partialOk = partialOk;
            _query = query;
            _serializer = serializer;
            _skip = skip;
            _snapshot = snapshot;
            _sort = sort;
            _tailableCursor = tailableCursor;
        }

        // properties
        public BsonDocument AdditionalOptions
        {
            get { return _additionalOptions; }
        }

        public bool AwaitData
        {
            get { return _awaitData; }
        }

        public int BatchSize
        {
            get { return _batchSize; }
        }

        public string CollectionName
        {
            get { return _collectionName; }
        }

        public string Comment
        {
            get { return _comment; }
        }

        public string DatabaseName
        {
            get { return _databaseName; }
        }

        public BsonDocument Fields
        {
            get { return _fields; }
        }

        public string Hint
        {
            get { return _hint; }
        }

        public int? Limit
        {
            get { return _limit; }
        }

        public bool NoCursorTimeout
        {
            get { return _noCursorTimeout; }
        }

        public bool PartialOk
        {
            get { return _partialOk; }
        }

        public BsonDocument Query
        {
            get { return _query; }
        }

        public IBsonSerializer<TDocument> Serializer
        {
            get { return _serializer; }
        }

        public int Skip
        {
            get { return _skip; }
        }

        public bool? Snapshot
        {
            get { return _snapshot; }
        }

        public BsonDocument Sort
        {
            get { return _sort; }
        }

        public bool TailableCursor
        {
            get { return _tailableCursor; }
        }

        // methods
        private QueryMessage CreateMessage(INode node)
        {
            var wrappedQuery = CreateWrappedQuery(node);
            var slaveOk = !(node is IWritableNode) || (node.Cluster is DirectCluster);

            int firstBatchSize;
            var limit = _limit ?? 0;
            if (limit < 0)
            {
                firstBatchSize = limit;
            }
            else if (limit == 0)
            {
                firstBatchSize = _batchSize;
            }
            else if (_batchSize == 0)
            {
                firstBatchSize = limit;
            }
            else if (limit < _batchSize)
            {
                firstBatchSize = limit;
            }
            else
            {
                firstBatchSize = _batchSize;
            }

            return new QueryMessage(
                RequestMessage.GetNextRequestId(),
                _databaseName,
                _collectionName,
                wrappedQuery,
                _fields,
                _skip,
                firstBatchSize,
                slaveOk,
                _partialOk,
                _noCursorTimeout,
                _tailableCursor,
                _awaitData);
        }

        private BsonDocument CreateWrappedQuery(INode node)
        {
            var shardedClusterNode = node as ShardedClusterNode;

            var wrappedQuery = new BsonDocument
            {
                { "$query", _query ?? new BsonDocument() },
                { "$orderby", () =>_sort, _sort != null },
                { "$hint", () => _hint, _hint != null },
                { "$snapshot", () => _snapshot.Value, _snapshot.HasValue },
                { "$comment", () => _comment, _comment != null },
                { "$readPreference", () => shardedClusterNode.CreateReadPreferenceDocument(), shardedClusterNode != null }
            };
            if (_additionalOptions != null)
            {
                wrappedQuery.AddRange(_additionalOptions);
            }
            return wrappedQuery;
        }

        public async Task<BatchCursor<TDocument>> ExecuteAsync(IReadableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var slidingTimeout = new SlidingTimeout(timeout);

            var message = CreateMessage(connection.Node);
            await connection.SendMessageAsync(message, slidingTimeout, cancellationToken);
            var reply = await connection.ReceiveMessageAsync<TDocument>(message.RequestId, _serializer, slidingTimeout, cancellationToken);
            return ProcessReply(reply, connection, slidingTimeout, cancellationToken);
        }

        public async Task<BsonDocument> ExplainAsync(IReadableConnection connection, bool verbose = false, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var additionalOptions = new BsonDocument();
            if (_additionalOptions != null)
            {
                additionalOptions.AddRange(_additionalOptions);
            }
            additionalOptions.Add("$explain", true);

            var operation = this
                .WithAdditionalOptions(additionalOptions)
                .WithLimit(-Math.Abs(_limit ?? 0))
                .WithSerializer<BsonDocument>(BsonDocumentSerializer.Instance);
            var cursor = await operation.ExecuteAsync(connection, timeout, cancellationToken);
            await cursor.MoveNextAsync();
            return cursor.Current.First();
        }

        private BatchCursor<TDocument> ProcessReply(ReplyMessage<TDocument> reply, IReadableConnection connection, SlidingTimeout slidingTimeout, CancellationToken cancellationToken)
        {
            if (reply.QueryFailure)
            {
                throw new CommandException("Query reply had QueryFailure flag set.", _query, reply.QueryFailureDocument);
            }

            var firstBatch = reply.Documents;
            var cursorId = reply.CursorId;
            return new BatchCursor<TDocument>(connection, _databaseName, _collectionName, _query, firstBatch, cursorId, _batchSize, _serializer, slidingTimeout, cancellationToken);
        }

        public FindOperation<TDocument> WithAdditionalOptions(BsonDocument value)
        {
            return object.ReferenceEquals(_additionalOptions, value) ? this : new Builder(this) { _additionalOptions = value }.Build();
        }

        public FindOperation<TDocument> WithAwaitData(bool value)
        {
            return (_awaitData == value) ? this : new Builder(this) { _awaitData = value }.Build();
        }

        public FindOperation<TDocument> WithBatchSize(int value)
        {
            return (_batchSize == value) ? this : new Builder(this) { _batchSize = value }.Build();
        }

        public FindOperation<TDocument> WithCollectionName(string value)
        {
            return (_collectionName == value) ? this : new Builder(this) { _collectionName = value }.Build();
        }

        public FindOperation<TDocument> WithDatabaseName(string value)
        {
            return (_databaseName == value) ? this : new Builder(this) { _databaseName = value }.Build();
        }

        public FindOperation<TDocument> WithFields(BsonDocument value)
        {
            return object.ReferenceEquals(_fields, value) ? this : new Builder(this) { _fields = value }.Build();
        }

        public FindOperation<TDocument> WithHint(BsonDocument keys)
        {
            var indexName = CreateIndexOperation.GetDefaultIndexName(keys);
            return WithHint(indexName);
        }

        public FindOperation<TDocument> WithHint(string value)
        {
            return object.Equals(_hint, value) ? this : new Builder(this) { _hint = value }.Build();
        }

        public FindOperation<TDocument> WithLimit(int? value)
        {
            return (_limit == value) ? this : new Builder(this) { _limit = value }.Build();
        }

        public FindOperation<TDocument> WithNoCursorTimeout(bool value)
        {
            return (_noCursorTimeout == value) ? this : new Builder(this) { _noCursorTimeout = value }.Build();
        }

        public FindOperation<TDocument> WithPartialOk(bool value)
        {
            return (_partialOk == value) ? this : new Builder(this) { _partialOk = value }.Build();
        }

        public FindOperation<TDocument> WithQuery(BsonDocument value)
        {
            return object.ReferenceEquals(_query, value) ? this : new Builder(this) { _query = value }.Build();
        }

        public FindOperation<TDocument> WithSerializer(IBsonSerializer<TDocument> value)
        {
            return object.ReferenceEquals(_serializer, value) ? this : new Builder(this) { _serializer = value }.Build();
        }

        public FindOperation<TOther> WithSerializer<TOther>(IBsonSerializer<TOther> value)
        {
            return new FindOperation<TOther>(
                _additionalOptions,
                _awaitData,
                _batchSize,
                _collectionName,
                _comment,
                _databaseName,
                _fields,
                _hint,
                _limit,
                _noCursorTimeout,
                _partialOk,
                _query,
                value,
                _skip,
                _snapshot,
                _sort,
                _tailableCursor);
        }

        public FindOperation<TDocument> WithSkip(int value)
        {
            return (_skip == value) ? this : new Builder(this) { _skip = value }.Build();
        }

        public FindOperation<TDocument> WithSnapshot(bool? value)
        {
            return (_snapshot == value) ? this : new Builder(this) { _snapshot = value }.Build();
        }

        public FindOperation<TDocument> WithSort(BsonDocument value)
        {
            return object.ReferenceEquals(_sort, value) ? this : new Builder(this) { _sort = value }.Build();
        }

        public FindOperation<TDocument> WithTailableCursor(bool value)
        {
            return (_tailableCursor == value) ? this : new Builder(this) { _tailableCursor = value }.Build();
        }

        // nested types
        private struct Builder
        {
            // fields
            public BsonDocument _additionalOptions;
            public bool _awaitData;
            public int _batchSize;
            public string _collectionName;
            public string _comment;
            public string _databaseName;
            public BsonDocument _fields;
            public string _hint;
            public int? _limit;
            public bool _noCursorTimeout;
            public bool _partialOk;
            public BsonDocument _query;
            public IBsonSerializer<TDocument> _serializer;
            public int _skip;
            public bool? _snapshot;
            public BsonDocument _sort;
            public bool _tailableCursor;

            // constructors
            public Builder(FindOperation<TDocument> other)
            {
                _additionalOptions = other.AdditionalOptions;
                _awaitData = other.AwaitData;
                _batchSize = other.BatchSize;
                _collectionName = other.CollectionName;
                _comment = other.Comment;
                _databaseName = other.DatabaseName;
                _fields = other.Fields;
                _hint = other.Hint;
                _limit = other.Limit;
                _noCursorTimeout = other.NoCursorTimeout;
                _partialOk = other.PartialOk;
                _query = other.Query;
                _serializer = other.Serializer;
                _skip = other.Skip;
                _snapshot = other.Snapshot;
                _sort = other.Sort;
                _tailableCursor = other.TailableCursor;
            }

            // methods
            public FindOperation<TDocument> Build()
            {
                return new FindOperation<TDocument>(
                    _additionalOptions,
                    _awaitData,
                    _batchSize,
                    _collectionName,
                    _comment,
                    _databaseName,
                    _fields,
                    _hint,
                    _limit,
                    _noCursorTimeout,
                    _partialOk,
                    _query,
                    _serializer,
                    _skip,
                    _snapshot,
                    _sort,
                    _tailableCursor);
            }
        }
    }
}
