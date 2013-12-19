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
    public class IndexExistsOperation : IOperation<bool>
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly string _indexName;

        // constructors
        public IndexExistsOperation(
            string databaseName,
            string collectionName,
            BsonDocument keys)
            : this(databaseName, collectionName, CreateIndexOperation.GetDefaultIndexName(keys))
        {
        }

        public IndexExistsOperation(
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
        public async Task<bool> ExecuteAsync(IReadableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = new BsonDocument
            {
                { "name", _indexName },
                { "ns", _databaseName + "." + _collectionName }
            };
            var operation = new CountOperation(_databaseName, "system.indexes", query);
            var count = await operation.ExecuteAsync(connection, timeout, cancellationToken);
            return (count != 0);
        }

        public IndexExistsOperation WithCollectionName(string value)
        {
            return (_collectionName == value) ? this : new IndexExistsOperation(_databaseName, value, _indexName);
        }

        public IndexExistsOperation WithDatabaseName(string value)
        {
            return (_databaseName == value) ? this : new IndexExistsOperation(value, _collectionName, _indexName);
        }

        public IndexExistsOperation WithIndexName(string value)
        {
            return (_indexName == value) ? this : new IndexExistsOperation(_databaseName, _collectionName, value);
        }

        public IndexExistsOperation WithKeys(BsonDocument value)
        {
            var indexName = CreateIndexOperation.GetDefaultIndexName(value);
            return WithIndexName(indexName);
        }
    }
}
