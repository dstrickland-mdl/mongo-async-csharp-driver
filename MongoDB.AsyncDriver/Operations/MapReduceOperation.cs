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
    public class MapReduceOperation : IOperation<IEnumerable<BsonValue>>
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly BsonJavaScript _finalizeFunction;
        private readonly bool? _javaScriptMode;
        private readonly long? _limit;
        private readonly BsonJavaScript _mapFunction;
        private readonly bool? _nonAtomicOutput;
        private readonly string _outputCollectionName;
        private readonly string _outputDatabaseName;
        private readonly MapReduceOutputMode _outputMode;
        private readonly BsonDocument _query;
        private readonly BsonJavaScript _reduceFunction;
        private readonly BsonDocument _scope;
        private readonly bool? _shardedOutput;
        private readonly BsonDocument _sort;
        private readonly bool? _verbose;

        // constructors
        public MapReduceOperation(string databaseName, string collectionName, BsonJavaScript mapFunction, BsonJavaScript reduceFunction, BsonDocument query = null)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _mapFunction = mapFunction;
            _reduceFunction = reduceFunction;
            _query = query;
        }

        private MapReduceOperation(
            string collectionName,
            string databaseName,
            BsonJavaScript finalizeFunction,
            bool? javaScriptMode,
            long? limit,
            BsonJavaScript mapFunction,
            bool? nonAtomicOutput,
            string outputCollectionName,
            string outputDatabaseName,
            MapReduceOutputMode outputMode,
            BsonDocument query,
            BsonJavaScript reduceFunction,
            BsonDocument scope,
            bool? shardedOutput,
            BsonDocument sort,
            bool? verbose)
        {
            _collectionName = collectionName;
            _databaseName = databaseName;
            _finalizeFunction = finalizeFunction;
            _javaScriptMode = javaScriptMode;
            _limit = limit;
            _mapFunction = mapFunction;
            _nonAtomicOutput = nonAtomicOutput;
            _outputCollectionName = outputCollectionName;
            _outputDatabaseName = outputDatabaseName;
            _outputMode = outputMode;
            _query = query;
            _reduceFunction = reduceFunction;
            _scope = scope;
            _shardedOutput = shardedOutput;
            _sort = sort;
            _verbose = verbose;
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

        public BsonJavaScript FinalizeFunction
        {
            get { return _finalizeFunction; }
        }

        public bool? JavaScriptMode
        {
            get { return _javaScriptMode; }
        }

        public long? Limit
        {
            get { return _limit; }
        }

        public BsonJavaScript MapFunction
        {
            get { return _mapFunction; }
        }

        public bool? NonAtomicOutput
        {
            get { return _nonAtomicOutput; }
        }

        public string OutputCollectionName
        {
            get { return _outputCollectionName; }
        }

        public string OutputDatabaseName
        {
            get { return _outputDatabaseName; }
        }

        public MapReduceOutputMode OutputMode
        {
            get { return _outputMode; }
        }

        public BsonDocument Query
        {
            get { return _query; }
        }

        public BsonJavaScript ReduceFunction
        {
            get { return _reduceFunction; }
        }

        public BsonDocument Scope
        {
            get { return _scope; }
        }

        public bool? ShardedOutput
        {
            get { return _shardedOutput; }
        }

        public BsonDocument Sort
        {
            get { return _sort; }
        }

        public bool? Verbose
        {
            get { return _verbose; }
        }

        // methods
        public BsonDocument CreateCommand()
        {
            BsonDocument output;
            if (_outputMode == MapReduceOutputMode.Inline)
            {
                output = new BsonDocument("inline", 1);
            }
            else
            {
                var action = _outputMode.ToString().ToLowerInvariant();
                output = new BsonDocument
                {
                    { action, _outputCollectionName },
                    { "db", _outputDatabaseName, _outputDatabaseName != null },
                    { "sharded", () => _shardedOutput.Value, _shardedOutput.HasValue },
                    { "nonAtomic", () => _nonAtomicOutput.Value, _nonAtomicOutput.HasValue }
                };
            }

            return new BsonDocument
            {
                { "mapReduce", _collectionName },
                { "map", _mapFunction },
                { "reduce", _reduceFunction },
                { "out" , output },
                { "query", _query, _query != null },
                { "sort", _sort, _sort != null },
                { "limit", () => _limit.Value, _limit.HasValue },
                { "finalize", _finalizeFunction, _finalizeFunction != null },
                { "scope", _scope, _scope != null },
                { "jsMode", () => _javaScriptMode.Value, _javaScriptMode.HasValue },
                { "verbose", () => _verbose.Value, _verbose.HasValue }
            };
        }

        public async Task<IEnumerable<BsonValue>> ExecuteAsync(IReadableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_outputMode != MapReduceOutputMode.Inline)
            {
                throw new InvalidOperationException("ExecuteAsync can only be called when OutputMode is Inline.");
            }

            var result = await ExecuteCommandAsync(connection, timeout, cancellationToken);
            return result["results"].AsBsonArray();
        }

        public async Task<BsonDocument> ExecuteCommandAsync(IReadableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = CreateCommand();
            var operation = new ReadCommandOperation(_databaseName, command);
            return await operation.ExecuteAsync(connection, timeout, cancellationToken);
        }

        public MapReduceOperation WithCollectionName(string value)
        {
            return (_collectionName == value) ? this : new Builder(this) { _collectionName = value }.Build();
        }

        public MapReduceOperation WithDatabaseName(string value)
        {
            return (_databaseName == value) ? this : new Builder(this) { _databaseName = value }.Build();
        }

        public MapReduceOperation WithFinalizeFunction(BsonJavaScript value)
        {
            return object.ReferenceEquals(_finalizeFunction, value) ? this : new Builder(this) { _finalizeFunction = value }.Build();
        }

        public MapReduceOperation WithJavaScriptMode(bool? value)
        {
            return (_javaScriptMode == value) ? this : new Builder(this) { _javaScriptMode = value }.Build();
        }

        public MapReduceOperation WithLimit(long? value)
        {
            return (_limit == value) ? this : new Builder(this) { _limit = value }.Build();
        }

        public MapReduceOperation WithMapFunction(BsonJavaScript value)
        {
            return object.ReferenceEquals(_mapFunction, value) ? this : new Builder(this) { _mapFunction = value }.Build();
        }

        public MapReduceOperation WithNonAtomicOutput(bool? value)
        {
            return (_nonAtomicOutput == value) ? this : new Builder(this) { _nonAtomicOutput = value }.Build();
        }

        public MapReduceOperation WithOutputCollectionName(string value)
        {
            return (_outputCollectionName == value) ? this : new Builder(this) { _outputCollectionName = value }.Build();
        }

        public MapReduceOperation WithOutputDatabaseName(string value)
        {
            return (_outputDatabaseName == value) ? this : new Builder(this) { _outputDatabaseName = value }.Build();
        }

        public MapReduceOperation WithOutputMode(MapReduceOutputMode value)
        {
            return (_outputMode == value) ? this : new Builder(this) { _outputMode = value }.Build();
        }

        public MapReduceOperation WithQuery(BsonDocument value)
        {
            return object.ReferenceEquals(_query, value) ? this : new Builder(this) { _query = value }.Build();
        }

        public MapReduceOperation WithReduceFunction(BsonJavaScript value)
        {
            return object.ReferenceEquals(_reduceFunction, value) ? this : new Builder(this) { _reduceFunction = value }.Build();
        }

        public MapReduceOperation WithScope(BsonDocument value)
        {
            return object.ReferenceEquals(_scope, value) ? this : new Builder(this) { _scope = value }.Build();
        }

        public MapReduceOperation WithShardedOutput(bool? value)
        {
            return (_shardedOutput == value) ? this : new Builder(this) { _shardedOutput = value }.Build();
        }

        public MapReduceOperation WithSort(BsonDocument value)
        {
            return object.ReferenceEquals(_sort, value) ? this : new Builder(this) { _sort = value }.Build();
        }

        public MapReduceOperation WithVerbose(bool? value)
        {
            return (_verbose == value) ? this : new Builder(this) { _verbose = value }.Build();
        }

        // nested types
        private struct Builder
        {
            // fields
            public string _collectionName;
            public string _databaseName;
            public BsonJavaScript _finalizeFunction;
            public bool? _javaScriptMode;
            public long? _limit;
            public BsonJavaScript _mapFunction;
            public bool? _nonAtomicOutput;
            public string _outputCollectionName;
            public string _outputDatabaseName;
            public MapReduceOutputMode _outputMode;
            public BsonDocument _query;
            public BsonJavaScript _reduceFunction;
            public BsonDocument _scope;
            public bool? _shardedOutput;
            public BsonDocument _sort;
            public bool? _verbose;

            // constructors
            public Builder(MapReduceOperation other)
            {
                _collectionName = other.CollectionName;
                _databaseName = other.DatabaseName;
                _finalizeFunction = other.FinalizeFunction;
                _javaScriptMode = other.JavaScriptMode;
                _limit = other.Limit;
                _mapFunction = other.MapFunction;
                _nonAtomicOutput = other.NonAtomicOutput;
                _outputCollectionName = other.OutputCollectionName;
                _outputDatabaseName = other.OutputDatabaseName;
                _outputMode = other.OutputMode;
                _query = other.Query;
                _reduceFunction = other.ReduceFunction;
                _scope = other.Scope;
                _shardedOutput = other.ShardedOutput;
                _sort = other.Sort;
                _verbose = other.Verbose;
            }

            // methods
            public MapReduceOperation Build()
            {
                return new MapReduceOperation(
                    _collectionName,
                    _databaseName,
                    _finalizeFunction,
                    _javaScriptMode,
                    _limit,
                    _mapFunction,
                    _nonAtomicOutput,
                    _outputCollectionName,
                    _outputDatabaseName,
                    _outputMode,
                    _query,
                    _reduceFunction,
                    _scope,
                    _shardedOutput,
                    _sort,
                    _verbose);
            }
        }
    }
}
