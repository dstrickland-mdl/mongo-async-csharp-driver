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
    public class DeleteOpcodeOperation : IOperation<BsonDocument>
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly bool _isMulti;
        private readonly BsonDocument _query;
        private readonly WriteConcern _writeConcern;

        // constructors
        public DeleteOpcodeOperation(
            string databaseName,
            string collectionName,
            BsonDocument query)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _query = query;
            _writeConcern = WriteConcern.Default;
        }

        private DeleteOpcodeOperation(
            string collectionName,
            string databaseName,
            bool isMulti,
            BsonDocument query,
            WriteConcern writeConcern)
        {
            _collectionName = collectionName;
            _databaseName = databaseName;
            _isMulti = isMulti;
            _query = query;
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

        public BsonDocument Query
        {
            get { return _query; }
        }

        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
        }

        // methods
        private DeleteMessage CreateMessage()
        {
            return new DeleteMessage(
                RequestMessage.GetNextRequestId(),
                _databaseName,
                _collectionName,
                _query,
                _isMulti);
        }

        public async Task<BsonDocument> ExecuteAsync(IWritableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var slidingTimeout = new SlidingTimeout(timeout);

            var deleteMessage = CreateMessage();
            if (_writeConcern == null)
            {
                await connection.SendMessageAsync(deleteMessage, slidingTimeout, cancellationToken);
                return null;
            }
            else
            {
                var getLastErrorMessage = GetLastError.CreateMessage(_databaseName, _writeConcern);
                await connection.SendMessagesAsync(new RequestMessage[] { deleteMessage, getLastErrorMessage }, slidingTimeout, cancellationToken);
                var reply = await connection.ReceiveMessageAsync<BsonDocument>(getLastErrorMessage.RequestId, BsonDocumentSerializer.Instance, slidingTimeout, cancellationToken);
                return GetLastError.ProcessReply(reply);
            }
        }

        public DeleteOpcodeOperation WithCollectionName(string value)
        {
            return (_collectionName == value) ? this : new Builder(this) { _collectionName = value }.Build();
        }

        public DeleteOpcodeOperation WithDatabaseName(string value)
        {
            return (_databaseName == value) ? this : new Builder(this) { _databaseName = value }.Build();
        }

        public DeleteOpcodeOperation WithIsMulti(bool value)
        {
            return (_isMulti == value) ? this : new Builder(this) { _isMulti = value }.Build();
        }

        public DeleteOpcodeOperation WithQuery(BsonDocument value)
        {
            return object.Equals(_query, value) ? this : new Builder(this) { _query = value }.Build();
        }

        public DeleteOpcodeOperation WithWriteConcern(WriteConcern value)
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
            public BsonDocument _query;
            public WriteConcern _writeConcern;

            // constructors
            public Builder(DeleteOpcodeOperation other)
            {
                _collectionName = other.CollectionName;
                _databaseName = other.DatabaseName;
                _isMulti = other.IsMulti;
                _query = other.Query;
                _writeConcern = other.WriteConcern;
            }
            
            // methods
            public DeleteOpcodeOperation Build()
            {
                return new DeleteOpcodeOperation(
                    _collectionName,
                    _databaseName,
                    _isMulti,
                    _query,
                    _writeConcern);
            }
        }
    }
}
