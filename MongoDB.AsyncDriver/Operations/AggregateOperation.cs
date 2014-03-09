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
    public class AggregateOperation : IReadOperation<BatchCursor<BsonDocument>>
    {
        // fields
        private readonly bool? _allowDiskUsage;
        private readonly int? _batchSize;
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly AggregateOutputMode _outputMode;
        private readonly IReadOnlyList<BsonDocument> _pipeline;

        // constructors
        public AggregateOperation(string databaseName, string collectionName, IEnumerable<BsonDocument> pipeline)
        {
            if (databaseName == null) { throw new ArgumentNullException("databaseName"); }
            if (databaseName == "") { throw new ArgumentException("Database name is empty.", "databaseName"); }
            if (collectionName == null) { throw new ArgumentNullException("collectionName"); }
            if (collectionName == "") { throw new ArgumentException("Collection name is empty.", "collectionName"); }
            if (pipeline == null) { throw new ArgumentNullException("pipeline"); }

            _databaseName = databaseName;
            _collectionName = collectionName;
            _pipeline = pipeline.ToList();
        }

        private AggregateOperation(
            bool? allowDiskUsage,
            int? batchSize,
            string collectionName,
            string databaseName,
            AggregateOutputMode outputMode,
            IReadOnlyList<BsonDocument> pipeline)
        {
            _allowDiskUsage = allowDiskUsage;
            _batchSize = batchSize;
            _collectionName = collectionName;
            _databaseName = databaseName;
            _outputMode = outputMode;
            _pipeline = pipeline;
        }

        // properties
        public bool? AllowDiskUsage
        {
            get { return _allowDiskUsage; }
        }

        public int? BatchSize
        {
            get { return _batchSize; }
        }

        public string CollectionName
        {
            get { return _collectionName; }
        }

        public string DatabaseName
        {
            get { return _databaseName; }
        }

        public AggregateOutputMode OutputMode
        {
            get { return _outputMode; }
        }

        public IReadOnlyList<BsonDocument> Pipeline
        {
            get { return _pipeline; }
        }

        // methods
        public BsonDocument CreateCommand()
        {
            BsonDocument cursor = null;
            if (_outputMode == AggregateOutputMode.Cursor)
            {
                cursor = new BsonDocument
                {
                    { "batchSize", () => _batchSize.Value, _batchSize.HasValue }
                };
            }

            return new BsonDocument
            {
                { "aggregate", _collectionName },
                { "pipeline", new BsonArray(_pipeline) },
                { "allowDiskUsage", () => _allowDiskUsage.Value, _allowDiskUsage.HasValue },
                { "cursor", cursor, cursor != null }
            };
        }

        public async Task<BatchCursor<BsonDocument>> ExecuteAsync(IReadableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connection == null) { throw new ArgumentNullException("connection");  }
            if (_outputMode != AggregateOutputMode.Cursor)
            {
                throw new ArgumentException("OutputMode is not Cursor.");
            }

            var slidingTimeout = new SlidingTimeout(timeout);
            var command = CreateCommand();
            var operation = new ReadCommandOperation(_databaseName, command);
            var result = await operation.ExecuteAsync(connection, slidingTimeout, cancellationToken);

            var firstBatch = ((BsonArray)result["cursor"]["firstBatch"]).Cast<BsonDocument>().ToList();
            var cursorId = result["cursor"]["id"].ToInt64();
            var serializer = BsonDocumentSerializer.Instance;
            return new BatchCursor<BsonDocument>(connection, _databaseName, _collectionName, command, firstBatch, cursorId, _batchSize ?? 0, serializer, slidingTimeout, cancellationToken);
        }

        public async Task<IReadOnlyList<BsonDocument>> ExecuteInlineAsync(IReadableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connection == null) { throw new ArgumentNullException("connection"); }
            if (_outputMode != AggregateOutputMode.Inline)
            {
                throw new ArgumentException("OutputMode is not Inline.");
            }

            var command = CreateCommand();
            var operation = new ReadCommandOperation(_databaseName, command);
            var result = await operation.ExecuteAsync(connection, timeout, cancellationToken);

            return result["result"].AsBsonArray().Cast<BsonDocument>().ToList();
        }

        public async Task ExecuteOutputToCollectionAsync(IWritableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connection == null) { throw new ArgumentNullException("connection"); }
            if (_outputMode != AggregateOutputMode.Collection)
            {
                throw new ArgumentException("OutputMode is not Collection.");
            }

            var command = CreateCommand();
            var operation = new WriteCommandOperation(_databaseName, command);
            await operation.ExecuteAsync(connection, timeout, cancellationToken);
        }

        public async Task<BsonDocument> ExplainAsync(IReadableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connection == null) { throw new ArgumentNullException("connection"); }

            var command = CreateCommand();
            command["explain"] = true;
            var operation = new ReadCommandOperation(_databaseName, command);
            return await operation.ExecuteAsync(connection, timeout, cancellationToken);
        }

        public AggregateOperation WithAllowDiskUsage(bool? value)
        {
            return (_allowDiskUsage == value) ? this : new Builder(this) { _allowDiskUsage = value }.Build();
        }

        public AggregateOperation WithBatchSize(int? value)
        {
            return (_batchSize == value) ? this : new Builder(this) { _batchSize = value }.Build();
        }

        public AggregateOperation WithCollectionName(string value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }
            if (value == "") { throw new ArgumentException("Collection name is empty.", "value"); }
            return (_collectionName == value) ? this : new Builder(this) { _collectionName = value }.Build();
        }

        public AggregateOperation WithDatabaseName(string value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }
            if (value == "") { throw new ArgumentException("Database name is empty.", "value"); }
            return (_databaseName == value) ? this : new Builder(this) { _databaseName = value }.Build();
        }

        public AggregateOperation WithOutputMode(AggregateOutputMode value)
        {
            return (_outputMode == value) ? this : new Builder(this) { _outputMode = value }.Build();
        }

        public AggregateOperation WithPipeline(IReadOnlyList<BsonDocument> value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }
            return (_pipeline == value) ? this : new Builder(this) { _pipeline = value }.Build();
        }

        // nested types
        private struct Builder
        {
            // fields
            public bool? _allowDiskUsage;
            public int? _batchSize;
            public string _collectionName;
            public string _databaseName;
            public AggregateOutputMode _outputMode;
            public IReadOnlyList<BsonDocument> _pipeline;

            // constructors
            public Builder(AggregateOperation other)
            {
                _allowDiskUsage = other.AllowDiskUsage;
                _batchSize = other.BatchSize;
                _collectionName = other.CollectionName;
                _databaseName = other.DatabaseName;
                _outputMode = other.OutputMode;
                _pipeline = other.Pipeline;
            }

            // methods
            public AggregateOperation Build()
            {
                return new AggregateOperation(
                    _allowDiskUsage,
                    _batchSize,
                    _collectionName,
                    _databaseName,
                    _outputMode,
                    _pipeline);
            }
        }
    }
}
