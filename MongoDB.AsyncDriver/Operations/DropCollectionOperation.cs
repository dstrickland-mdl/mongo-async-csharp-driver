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
    public class DropCollectionOperation : IOperation<BsonDocument>
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;

        // constructors
        public DropCollectionOperation(
            string databaseName,
            string collectionName)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
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

        // methods
        public BsonDocument CreateCommand()
        {
            return new BsonDocument { { "drop", _collectionName } };
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

        public DropCollectionOperation WithCollectionName(string value)
        {
            return (_collectionName != value) ? this : new DropCollectionOperation(_databaseName, value);
        }

        public DropCollectionOperation WithDatabaseName(string value)
        {
            return (_databaseName != value) ? this : new DropCollectionOperation(value, _collectionName);
        }
    }
}
