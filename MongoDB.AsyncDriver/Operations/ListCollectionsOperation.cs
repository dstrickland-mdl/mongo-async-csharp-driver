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
    public class ListCollectionsOperation : IOperation<List<string>>
    {
        // fields
        private readonly string _databaseName;

        // constructors
        public ListCollectionsOperation(
            string databaseName)
        {
            _databaseName = databaseName;
        }

        // properties
        public string DatabaseName
        {
            get { return _databaseName; }
        }

        // methods
        public async Task<List<string>> ExecuteAsync(IReadableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var operation = new FindOperation(_databaseName, "system.namespaces");
            var cursor = await operation.ExecuteAsync(connection, timeout, cancellationToken);
            var result = new List<string>();
            var prefix = _databaseName + ".";
            while (await cursor.MoveNextAsync())
            {
                var document = cursor.Current;
                var name = (string)document["name"];
                if (name.StartsWith(prefix))
                {
                    var collectionName = name.Substring(prefix.Length);
                    if (!collectionName.Contains('$') || collectionName.EndsWith(".oplog.$"))
                    {
                        result.Add(collectionName);
                    }
                }
            }
            return result;
        }

        public ListCollectionsOperation WithDatabaseName(string value)
        {
            return (_databaseName == value) ? this : new ListCollectionsOperation(value);
        }
    }
}
