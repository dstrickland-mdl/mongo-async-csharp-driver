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
    public class CountOperation : IReadOperation<long>
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly long? _limit;
        private readonly BsonDocument _query;
        private readonly long? _skip;

        // constructors
        public CountOperation(string databaseName, string collectionName, BsonDocument query = null)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _query = query;
        }

        private CountOperation(
            string collectionName,
            string databaseName,
            long? limit,
            BsonDocument query,
            long? skip)
        {
            _collectionName = collectionName;
            _databaseName = databaseName;
            _limit = limit;
            _query = query;
            _skip = skip;
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

        public long? Limit
        {
            get { return _limit; }
        }

        public BsonDocument Query
        {
            get { return _query; }
        }

        public long? Skip
        {
            get { return _skip; }
        }

        // methods
        public BsonDocument CreateCommand()
        {
            return new BsonDocument
            {
                { "count", _collectionName },
                { "query", _query, _query != null },
                { "limit", () => _limit.Value, _limit.HasValue },
                { "skip", () => _skip.Value, _skip.HasValue }
            };
        }

        public async Task<long> ExecuteAsync(IReadableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var document = await ExecuteCommandAsync(connection, timeout, cancellationToken);
            return document["n"].ToInt64();
        }

        public async Task<BsonDocument> ExecuteCommandAsync(IReadableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = CreateCommand();
            var operation = new ReadCommandOperation(_databaseName, command);
            return await operation.ExecuteAsync(connection, timeout, cancellationToken);
        }

        public CountOperation WithCollectionName(string value)
        {
            return (_collectionName == value) ? this : new Builder(this) { _collectionName = value }.Build();
        }

        public CountOperation WithDatabaseName(string value)
        {
            return (_databaseName == value) ? this : new Builder(this) { _databaseName = value }.Build();
        }

        public CountOperation WithLimit(long? value)
        {
            return (_limit == value) ? this : new Builder(this) { _limit = value }.Build();
        }

        public CountOperation WithQuery(BsonDocument value)
        {
            return (_query == value) ? this : new Builder(this) { _query = value }.Build();
        }

        public CountOperation WithSkip(long? value)
        {
            return (_skip == value) ? this : new Builder(this) { _skip = value }.Build();
        }

        // nested types
        private struct Builder
        {
            // fields
            public string _collectionName;
            public string _databaseName;
            public long? _limit;
            public BsonDocument _query;
            public long? _skip;

            // constructors
            public Builder(CountOperation other)
            {
                _collectionName = other.CollectionName;
                _databaseName = other.DatabaseName;
                _limit = other.Limit;
                _query = other.Query;
                _skip = other.Skip;
            }

            // methods
            public CountOperation Build()
            {
                return new CountOperation(
                    _collectionName,
                    _databaseName,
                    _limit,
                    _query,
                    _skip);
            }
        }
    }
}
