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
    public class DistinctOperation : IReadOperation<IEnumerable<BsonValue>>
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly string _key;
        private readonly BsonDocument _query;

        // constructors
        public DistinctOperation(string databaseName, string collectionName, string key, BsonDocument query = null)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _key = key;
            _query = query;
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

        public string Key
        {
            get { return _key; }
        }

        public BsonDocument Query
        {
            get { return _query; }
        }

        // methods
        public BsonDocument CreateCommand()
        {
            return new BsonDocument
            {
                { "distinct", _collectionName },
                { "key", _key },
                { "query", _query, _query != null }
            };
        }

        public async Task<IEnumerable<BsonValue>> ExecuteAsync(IReadableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var document = await ExecuteCommandAsync(connection, timeout, cancellationToken);
            return document["values"].AsBsonArray();
        }

        public async Task<BsonDocument> ExecuteCommandAsync(IReadableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = CreateCommand();
            var operation = new ReadCommandOperation(_databaseName, command);
            return await operation.ExecuteAsync(connection, timeout, cancellationToken);
        }

        public DistinctOperation WithCollectionName(string value)
        {
            return (_collectionName == value) ? this : new DistinctOperation(_databaseName, value, _key, _query);
        }

        public DistinctOperation WithDatabaseName(string value)
        {
            return (_collectionName == value) ? this : new DistinctOperation(value, _collectionName, _key, _query);
        }

        public DistinctOperation WithKey(string value)
        {
            return (_collectionName == value) ? this : new DistinctOperation(_databaseName, _collectionName, value, _query);
        }

        public DistinctOperation WithQuery(BsonDocument value)
        {
            return object.Equals(_query, value) ? this : new DistinctOperation(_databaseName, _collectionName, _key, value);
        }
    }
}
