﻿/* Copyright 2013-2014 MongoDB Inc.
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
    public class FindAndModifyOperation : IOperation<BsonDocument>
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly FindAndModifyDocumentVersion? _documentVersionReturned;
        private readonly BsonDocument _fields;
        private readonly BsonDocument _query;
        private readonly BsonDocument _sort;
        private readonly BsonDocument _update;
        private readonly bool? _upsert;

        // constructors
        public FindAndModifyOperation(
            string databaseName,
            string collectionName,
            BsonDocument query,
            BsonDocument update,
            BsonDocument sort = null)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _query = query;
            _update = update;
            _sort = sort;
        }

        private FindAndModifyOperation(
            string collectionName,
            string databaseName,
            FindAndModifyDocumentVersion? documentVersionReturned,
            BsonDocument fields,
            BsonDocument query,
            BsonDocument sort,
            BsonDocument update,
            bool? upsert)
        {
            _collectionName = collectionName;
            _databaseName = databaseName;
            _documentVersionReturned = documentVersionReturned;
            _fields = fields;
            _query = query;
            _sort = sort;
            _update = update;
            _upsert = upsert;
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

        public FindAndModifyDocumentVersion? DocumentVersionReturned
        {
            get { return _documentVersionReturned; }
        }

        public BsonDocument Fields
        {
            get { return _fields; }
        }

        public BsonDocument Query
        {
            get { return _query; }
        }

        public BsonDocument Sort
        {
            get { return _sort; }
        }

        public BsonDocument Update
        {
            get { return _update; }
        }

        public bool? Upsert
        {
            get { return _upsert; }
        }

        // methods
        public BsonDocument CreateCommand()
        {
            return new BsonDocument
            {
                { "findAndModify", _collectionName },
                { "query", _query, _query != null },
                { "sort", _sort, _sort != null },
                { "update", _update },
                { "new", () => _documentVersionReturned.Value == FindAndModifyDocumentVersion.Modified, _documentVersionReturned.HasValue },
                { "field", _fields, _fields != null },
                { "upsert", () => _upsert.Value, _upsert.HasValue }
            };
        }

        public async Task<BsonDocument> ExecuteAsync(IWritableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await ExecuteCommandAsync(connection, timeout, cancellationToken);
            return result["value"].AsBsonDocument();
        }

        public async Task<BsonDocument> ExecuteCommandAsync(IWritableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = CreateCommand();
            var operation = new WriteCommandOperation(_databaseName, command);
            return await operation.ExecuteAsync(connection, timeout, cancellationToken);
        }

        public FindAndModifyOperation WithCollectionName(string value)
        {
            return (_collectionName == value) ? this : new Builder(this) { _collectionName = value }.Build();
        }

        public FindAndModifyOperation WithDatabaseName(string value)
        {
            return (_databaseName == value) ? this : new Builder(this) { _databaseName = value }.Build();
        }

        public FindAndModifyOperation WithDocumentVersionReturned(FindAndModifyDocumentVersion? value)
        {
            return (_documentVersionReturned == value) ? this : new Builder(this) { _documentVersionReturned = value }.Build();
        }

        public FindAndModifyOperation WithFields(BsonDocument value)
        {
            return object.ReferenceEquals(_fields, value) ? this : new Builder(this) { _fields = value }.Build();
        }

        public FindAndModifyOperation WithQuery(BsonDocument value)
        {
            return object.ReferenceEquals(_query, value) ? this : new Builder(this) { _query = value }.Build();
        }

        public FindAndModifyOperation WithSort(BsonDocument value)
        {
            return object.ReferenceEquals(_sort, value) ? this : new Builder(this) { _sort = value }.Build();
        }

        public FindAndModifyOperation WithUpdate(BsonDocument value)
        {
            return object.ReferenceEquals(_update, value) ? this : new Builder(this) { _update = value }.Build();
        }

        public FindAndModifyOperation WithUpsert(bool? value)
        {
            return (_upsert == value) ? this : new Builder(this) { _upsert = value }.Build();
        }

        // nested types
        private struct Builder
        {
            // fields
            public string _collectionName;
            public string _databaseName;
            public FindAndModifyDocumentVersion? _documentVersionReturned;
            public BsonDocument _fields;
            public BsonDocument _query;
            public BsonDocument _sort;
            public BsonDocument _update;
            public bool? _upsert;

            // constructors
            public Builder(FindAndModifyOperation other)
            {
                _collectionName = other.CollectionName;
                _databaseName = other.DatabaseName;
                _documentVersionReturned = other.DocumentVersionReturned;
                _fields = other.Fields;
                _query = other.Query;
                _sort = other.Sort;
                _update = other.Update;
                _upsert = other.Upsert;
            }

            // methods
            public FindAndModifyOperation Build()
            {
                return new FindAndModifyOperation(
                    _collectionName,
                    _databaseName,
                    _documentVersionReturned,
                    _fields,
                    _query,
                    _sort,
                    _update,
                    _upsert);
            }
        }
    }
}
