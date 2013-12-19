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
    public class DeleteCommandOperation : IOperation<BsonDocument>
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly IEnumerable<DeleteRequest> _deletes;
        private readonly int? _maxBatchCount;
        private readonly int? _maxBatchLength;
        private readonly int? _maxDocumentSize;
        private readonly int? _maxWireDocumentSize;
        private readonly bool _ordered = true;
        private readonly WriteConcern _writeConcern = WriteConcern.Default;

        // constructors
        public DeleteCommandOperation(
            string databaseName,
            string collectionName,
            IEnumerable<DeleteRequest> deletes)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _deletes = deletes;
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

        public IEnumerable<DeleteRequest> Deletes
        {
            get { return _deletes; }
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

        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
        }

        // methods
        public Task<BsonDocument> ExecuteAsync(IWritableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
