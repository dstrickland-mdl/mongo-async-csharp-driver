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
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public class Collection : Collection<BsonDocument>
    {
        // constructors
        public Collection(string databaseName, string collectionName)
            : base(databaseName, collectionName, BsonDocumentSerializer.Instance)
        {
        }
    }

    public class Collection<TDocument>
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly IBsonSerializer<TDocument> _serializer;

        // constructors
        public Collection(string databaseName, string collectionName, IBsonSerializer<TDocument> serializer)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _serializer = serializer;
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

        public IBsonSerializer<TDocument> Serializer
        {
            get { return _serializer; }
        }
    }
}
