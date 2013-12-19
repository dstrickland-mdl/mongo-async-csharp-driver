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
    public class UpdateCommandOperation : IOperation<BsonDocument>
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly int? _maxBatchCount;
        private readonly int? _maxBatchLength;
        private readonly int? _maxDocumentSize;
        private readonly int? _maxWireDocumentSize;
        private readonly bool _ordered;
        private readonly IEnumerable<UpdateRequest> _updates;
        private readonly WriteConcern _writeConcern;

        // constructors
        public UpdateCommandOperation(
            string databaseName,
            string collectionName,
            IEnumerable<UpdateRequest> updates)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _updates = updates;
            _ordered = true;
            _writeConcern = WriteConcern.Default;
        }

        private UpdateCommandOperation(
            string collectionName,
            string databaseName,
            int? maxBatchCount,
            int? maxBatchLength,
            int? maxDocumentSize,
            int? maxWireDocumentSize,
            bool ordered,
            IEnumerable<UpdateRequest> updates,
            WriteConcern writeConcern)
        {
            _collectionName = collectionName;
            _databaseName = databaseName;
            _maxBatchCount = maxBatchCount;
            _maxBatchLength = maxBatchLength;
            _maxDocumentSize = maxDocumentSize;
            _maxWireDocumentSize = maxWireDocumentSize;
            _ordered = ordered;
            _updates = updates;
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

        public int? MaxWireDocumentSize
        {
            get { return _maxWireDocumentSize; }
        }

        public bool Ordered
        {
            get { return _ordered; }
        }

        public IEnumerable<UpdateRequest> Updates
        {
            get { return _updates; }
        }

        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
        }

        // methods
        public Task<BsonDocument> ExecuteAsync(IWritableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public UpdateCommandOperation WithCollectionName(string value)
        {
            return (_collectionName == value) ? this : new Builder(this) { _collectionName = value }.Build();
        }

        public UpdateCommandOperation WithDatabaseName(string value)
        {
            return (_databaseName == value) ? this : new Builder(this) { _databaseName = value }.Build();
        }

        public UpdateCommandOperation WithMaxBatchCount(int? value)
        {
            return (_maxBatchCount == value) ? this : new Builder(this) { _maxBatchCount = value }.Build();
        }

        public UpdateCommandOperation WithMaxBatchLength(int? value)
        {
            return (_maxBatchLength == value) ? this : new Builder(this) { _maxBatchLength = value }.Build();
        }

        public UpdateCommandOperation WithMaxDocumentSize(int? value)
        {
            return (_maxDocumentSize == value) ? this : new Builder(this) { _maxDocumentSize = value }.Build();
        }

        public UpdateCommandOperation WithMaxWireDocumentSize(int? value)
        {
            return (_maxWireDocumentSize == value) ? this : new Builder(this) { _maxWireDocumentSize = value }.Build();
        }

        public UpdateCommandOperation WithOrdered(bool value)
        {
            return (_ordered == value) ? this : new Builder(this) { _ordered = value }.Build();
        }

        public UpdateCommandOperation WithUpdates(IEnumerable<UpdateRequest> value)
        {
            return object.ReferenceEquals(_updates, value) ? this : new Builder(this) { _updates = value }.Build();
        }

        public UpdateCommandOperation WithWriteConcern(WriteConcern value)
        {
            return object.Equals(_writeConcern, value) ? this : new Builder(this) { _writeConcern = value }.Build();
        }

        // nested types
        private struct Builder
        {
            // fields
            public string _collectionName;
            public string _databaseName;
            public int? _maxBatchCount;
            public int? _maxBatchLength;
            public int? _maxDocumentSize;
            public int? _maxWireDocumentSize;
            public bool _ordered;
            public IEnumerable<UpdateRequest> _updates;
            public WriteConcern _writeConcern;

            // constructors
            public Builder(UpdateCommandOperation other)
            {
                _collectionName = other.CollectionName;
                _databaseName = other.DatabaseName;
                _maxBatchCount = other.MaxBatchCount;
                _maxBatchLength = other.MaxBatchLength;
                _maxDocumentSize = other.MaxDocumentSize;
                _maxWireDocumentSize = other.MaxWireDocumentSize;
                _ordered = other.Ordered;
                _updates = other.Updates;
                _writeConcern = other.WriteConcern;
            }

            // methods
            public UpdateCommandOperation Build()
            {
                return new UpdateCommandOperation(
                    _collectionName,
                    _databaseName,
                    _maxBatchCount,
                    _maxBatchLength,
                    _maxDocumentSize,
                    _maxWireDocumentSize,
                    _ordered,
                    _updates,
                    _writeConcern);
            }
        }
    }
}
