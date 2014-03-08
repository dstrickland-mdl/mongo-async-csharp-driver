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
    public class InsertOpcodeOperation : InsertOpcodeOperation<BsonDocument>
    {
        // constructors
        public InsertOpcodeOperation(
            string databaseName,
            string collectionName,
            IEnumerable<BsonDocument> documents)
            : base(databaseName, collectionName, BsonDocumentSerializer.Instance, documents)
        {
        }
    }

    public class InsertOpcodeOperation<TDocument> : IOperation<BsonDocument>
    {
        // fields
        private readonly string _collectionName;
        private readonly bool _continueOnError;
        private readonly string _databaseName;
        private readonly IEnumerable<TDocument> _documents;
        private readonly int? _maxBatchCount;
        private readonly int? _maxBatchLength;
        private readonly int? _maxDocumentSize;
        private readonly IBsonSerializer<TDocument> _serializer;
        private readonly WriteConcern _writeConcern = WriteConcern.Default;

        // constructors
        public InsertOpcodeOperation(
            string databaseName,
            string collectionName,
            IBsonSerializer<TDocument> serializer,
            IEnumerable<TDocument> documents)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _serializer = serializer;
            _documents = documents;
        }

        private InsertOpcodeOperation(
            string collectionName,
            bool continueOnError,
            string databaseName,
            IEnumerable<TDocument> documents,
            int? maxBatchCount,
            int? maxBatchLength,
            int? maxDocumentSize,
            IBsonSerializer<TDocument> serializer,
            WriteConcern writeConcern)
        {
            _collectionName = collectionName;
            _continueOnError = continueOnError;
            _databaseName = databaseName;
            _documents = documents;
            _maxBatchCount = maxBatchCount;
            _maxBatchLength = maxBatchLength;
            _maxDocumentSize = maxDocumentSize;
            _serializer = serializer;
            _writeConcern = writeConcern;
        }

        // properties
        public string CollectionName
        {
            get { return _collectionName; }
        }

        public bool ContinueOnError
        {
            get { return _continueOnError; }
        }

        public string DatabaseName
        {
            get { return _databaseName; }
        }

        public IEnumerable<TDocument> Documents
        {
            get { return _documents; }
        }

        public int? MaxBatchCount
        {
            get { return _maxBatchCount; }
        }

        public int? MaxBatchLength
        {
            get { return _maxBatchLength; }
        }

        public int? MaxDocumentSize
        {
            get { return _maxDocumentSize; }
        }

        public IBsonSerializer<TDocument> Serializer
        {
            get { return _serializer; }
        }

        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
        }

        // methods
        private InsertMessage<TDocument> CreateMessage(IConnection connection, Batch<TDocument> batch)
        {
            return new InsertMessage<TDocument>(
                RequestMessage.GetNextRequestId(),
                _databaseName,
                _collectionName,
                _serializer,
                batch,
                _maxBatchCount ?? 10000,
                _maxBatchLength ?? connection.Node.Info.MaxMessageSize,
                _continueOnError);
        }

        public async Task<BsonDocument> ExecuteAsync(IWritableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var enumerator = _documents.GetEnumerator())
            {
                var batch = new FirstBatch<TDocument>(enumerator, false);
                return await ExecuteBatchAsync(connection, batch, timeout, cancellationToken);
            }
        }

        public async Task<BsonDocument> ExecuteBatchAsync(IConnection connection, Batch<TDocument> batch, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var slidingTimeout = new SlidingTimeout(timeout);

            var insertMessage = CreateMessage(connection, batch);
            if (_writeConcern == null)
            {
                await connection.SendMessageAsync(insertMessage, slidingTimeout, cancellationToken);
                return null;
            }
            else
            {
                var getLastErrorMessage = GetLastError.CreateMessage(_databaseName, _writeConcern);
                await connection.SendMessagesAsync(new RequestMessage[] { insertMessage, getLastErrorMessage }, slidingTimeout, cancellationToken);
                var reply = await connection.ReceiveMessageAsync<BsonDocument>(getLastErrorMessage.RequestId, BsonDocumentSerializer.Instance, slidingTimeout, cancellationToken);
                return GetLastError.ProcessReply(reply);
            }
        }

        public InsertOpcodeOperation<TDocument> WithCollectionName(string value)
        {
            return (_collectionName == value) ? this : new Builder(this) { _collectionName = value }.Build();
        }

        public InsertOpcodeOperation<TDocument> WithContinueOnError(bool value)
        {
            return (_continueOnError == value) ? this : new Builder(this) { _continueOnError = value }.Build();
        }

        public InsertOpcodeOperation<TDocument> WithDatabaseName(string value)
        {
            return (_databaseName == value) ? this : new Builder(this) { _databaseName = value }.Build();
        }

        public InsertOpcodeOperation<TDocument> WithDocuments(IEnumerable<TDocument> value)
        {
            return object.ReferenceEquals(_documents, value) ? this : new Builder(this) { _documents = value }.Build();
        }

        public InsertOpcodeOperation<TDocument> WithMaxBatchCount(int? value)
        {
            return (_maxBatchCount == value) ? this : new Builder(this) { _maxBatchCount = value }.Build();
        }

        public InsertOpcodeOperation<TDocument> WithMaxBatchLength(int? value)
        {
            return (_maxBatchLength == value) ? this : new Builder(this) { _maxBatchLength = value }.Build();
        }

        public InsertOpcodeOperation<TDocument> WithMaxDocumentSize(int? value)
        {
            return (_maxDocumentSize == value) ? this : new Builder(this) { _maxDocumentSize = value }.Build();
        }

        public InsertOpcodeOperation<TDocument> WithSerializer(IBsonSerializer<TDocument> value)
        {
            return object.ReferenceEquals(_serializer, value) ? this : new Builder(this) { _serializer = value }.Build();
        }

        public InsertOpcodeOperation<TOther> WithSerializer<TOther>(IBsonSerializer<TOther> value)
        {
            IEnumerable<TOther> documents = null;
            return new InsertOpcodeOperation<TOther>(
                _collectionName,
                _continueOnError,
                _databaseName,
                documents,
                _maxBatchCount,
                _maxBatchLength,
                _maxDocumentSize,
                value,
                _writeConcern);
        }

        public InsertOpcodeOperation<TDocument> WithWriteConcern(WriteConcern value)
        {
            return object.Equals(_writeConcern, value) ? this : new Builder(this) { _writeConcern = value }.Build();
        }

        // nested types
        private struct Builder
        {
            // fields
            public string _collectionName;
            public bool _continueOnError;
            public string _databaseName;
            public IEnumerable<TDocument> _documents;
            public int? _maxBatchCount;
            public int? _maxBatchLength;
            public int? _maxDocumentSize;
            public IBsonSerializer<TDocument> _serializer;
            public WriteConcern _writeConcern;

            // constructors
            public Builder(InsertOpcodeOperation<TDocument> original)
            {
                _collectionName = original._collectionName;
                _continueOnError = original._continueOnError;
                _databaseName = original._databaseName;
                _documents = original._documents;
                _maxBatchCount = original.MaxBatchCount;
                _maxBatchLength = original.MaxBatchLength;
                _maxDocumentSize = original.MaxDocumentSize;
                _serializer = original._serializer;
                _writeConcern = original.WriteConcern;
            }

            // methods
            public InsertOpcodeOperation<TDocument> Build()
            {
                return new InsertOpcodeOperation<TDocument>(
                    _collectionName,
                    _continueOnError,
                    _databaseName,
                    _documents,
                    _maxBatchCount,
                    _maxBatchLength,
                    _maxDocumentSize,
                    _serializer,
                    _writeConcern);
            }
        }
    }
}
