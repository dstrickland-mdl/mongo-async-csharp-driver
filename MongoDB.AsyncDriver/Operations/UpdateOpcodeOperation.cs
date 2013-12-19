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
    public class UpdateOpcodeOperation : IWriteOperation<BsonDocument>
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly bool _isMulti;
        private readonly bool _isUpsert;
        private readonly int? _maxDocumentSize;
        private readonly BsonDocument _query;
        private readonly BsonDocument _update;
        private readonly WriteConcern _writeConcern;

        // constructors
        public UpdateOpcodeOperation(
            string databaseName,
            string collectionName,
            BsonDocument query,
            BsonDocument update)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _query = query;
            _update = update;
            _writeConcern = WriteConcern.Default;
        }

        private UpdateOpcodeOperation(
            string collectionName,
            string databaseName,
            bool isMulti,
            bool isUpsert,
            int? maxDocumentSize,
            BsonDocument query,
            BsonDocument update,
            WriteConcern writeConcern)
        {
            _collectionName = collectionName;
            _databaseName = databaseName;
            _isMulti = isMulti;
            _isUpsert = isUpsert;
            _maxDocumentSize = maxDocumentSize;
            _query = query;
            _update = update;
            _writeConcern = writeConcern;
        }

       // properties
        public string CollectionName
        {
            get { return _collectionName; }
        }

        public string DatabaseName
        {
            get { return _databaseName; }
        }

        public bool IsMulti
        {
            get { return _isMulti; }
        }

        public bool IsUpsert
        {
            get { return _isUpsert; }
        }

        public int? MaxDocumentSize
        {
            get { return _maxDocumentSize; }
        }

        public BsonDocument Query
        {
            get { return _query; }
        }

        public BsonDocument Update
        {
            get { return _update; }
        }

        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
        }

        // methods
        private UpdateMessage CreateMessage()
        {
            return new UpdateMessage(
                RequestMessage.GetNextRequestId(),
                _databaseName,
                _collectionName,
                _query,
                _update,
                _isMulti,
                _isUpsert);
        }

        public async Task<BsonDocument> ExecuteAsync(IWritableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var slidingTimeout = new SlidingTimeout(timeout);

            var updateMessage = CreateMessage();
            if (_writeConcern == null)
            {
                await connection.SendMessageAsync(updateMessage, slidingTimeout, cancellationToken);
                return null;
            }
            else
            {
                var getLastErrorMessage = GetLastError.CreateMessage(_databaseName, _writeConcern);
                await connection.SendMessagesAsync(new RequestMessage[] { updateMessage, getLastErrorMessage }, slidingTimeout, cancellationToken);
                var reply = await connection.ReceiveMessageAsync<BsonDocument>(getLastErrorMessage.RequestId, BsonDocumentSerializer.Instance, slidingTimeout, cancellationToken);
                return GetLastError.ProcessReply(reply);
            }
        }

        public UpdateOpcodeOperation WithCollectionName(string value)
        {
            return (_collectionName == value) ? this : new Builder(this) { _collectionName = value }.Build();
        }

        public UpdateOpcodeOperation WithDatabaseName(string value)
        {
            return (_databaseName == value) ? this : new Builder(this) { _databaseName = value }.Build();
        }

        public UpdateOpcodeOperation WithIsMulti(bool value)
        {
            return (_isMulti == value) ? this : new Builder(this) { _isMulti = value }.Build();
        }

        public UpdateOpcodeOperation WithIsUpsert(bool value)
        {
            return (_isUpsert == value) ? this : new Builder(this) { _isUpsert = value }.Build();
        }

        public UpdateOpcodeOperation WithMaxDocumentSize(int? value)
        {
            return (_maxDocumentSize == value) ? this : new Builder(this) { _maxDocumentSize = value }.Build();
        }

        public UpdateOpcodeOperation WithQuery(BsonDocument value)
        {
            return object.ReferenceEquals(_query, value) ? this : new Builder(this) { _query = value }.Build();
        }

        public UpdateOpcodeOperation WithUpdate(BsonDocument value)
        {
            return object.ReferenceEquals(_update, value) ? this : new Builder(this) { _update = value }.Build();
        }

        public UpdateOpcodeOperation WithWriteConcern(WriteConcern value)
        {
            return object.Equals(_writeConcern, value) ? this : new Builder(this) { _writeConcern = value }.Build();
        }

        // nested types
        private struct Builder
        {
            // fields
            public string _collectionName;
            public string _databaseName;
            public bool _isMulti;
            public bool _isUpsert;
            public int? _maxDocumentSize;
            public BsonDocument _query;
            public BsonDocument _update;
            public WriteConcern _writeConcern;

            // constructors
            public Builder(UpdateOpcodeOperation other)
            {
                _collectionName = other.CollectionName;
                _databaseName = other.DatabaseName;
                _isMulti = other.IsMulti;
                _isUpsert = other.IsUpsert;
                _maxDocumentSize = other.MaxDocumentSize;
                _query = other.Query;
                _update = other.Update;
                _writeConcern = other.WriteConcern;
            }

            // methods
            public UpdateOpcodeOperation Build()
            {
                return new UpdateOpcodeOperation(
                    _collectionName,
                    _databaseName,
                    _isMulti,
                    _isUpsert,
                    _maxDocumentSize,
                    _query,
                    _update,
                    _writeConcern);
            }
        }
    }
}
