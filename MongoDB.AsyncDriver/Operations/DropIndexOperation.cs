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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public class DropIndexOperation : IOperation<BsonDocument>
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly string _indexName;

        // constructors
        public DropIndexOperation(
            string databaseName,
            string collectionName,
            BsonDocument keys)
            : this(databaseName, collectionName, CreateIndexOperation.GetDefaultIndexName(keys))
        {
        }

        public DropIndexOperation(
            string databaseName,
            string collectionName,
            string indexName)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _indexName = indexName;
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

        public string IndexName
        {
            get { return _indexName; }
        }

        // methods
        public BsonDocument CreateCommand()
        {
            return new BsonDocument
            {
                { "deleteIndexes", _collectionName },
                { "index", _indexName }
            };
        }

        public async Task<BsonDocument> ExecuteAsync(IWritableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = CreateCommand();
            var operation = new WriteCommandOperation(_databaseName, command);
            try
            {
                return await operation.ExecuteAsync(connection, timeout, cancellationToken);
            }
            catch (CommandException ex)
            {
                var result = ex.Result;
                if ((string)result["errmsg"] == "ns not found")
                {
                    return result;
                }
                throw;
            }
        }

        public DropIndexOperation WithCollectionName(string value)
        {
            return (_collectionName == value) ? this : new DropIndexOperation(_databaseName, value, _indexName);
        }

        public DropIndexOperation WithDatabaseName(string value)
        {
            return (_databaseName == value) ? this : new DropIndexOperation(value, _collectionName, _indexName);
        }

        public DropIndexOperation WithIndexName(string value)
        {
            return (_indexName == value) ? this : new DropIndexOperation(_databaseName, _collectionName, value);
        }

        public DropIndexOperation WithKeys(BsonDocument value)
        {
            var indexName = CreateIndexOperation.GetDefaultIndexName(value);
            return WithIndexName(indexName);
        }
    }
}
