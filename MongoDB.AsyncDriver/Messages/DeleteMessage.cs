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
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public class DeleteMessage : RequestMessage, IEncodableMessage<DeleteMessage>
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly bool _isMulti;
        private readonly BsonDocument _query;

        // constructors
        public DeleteMessage(
            int requestId,
            string databaseName,
            string collectionName,
            BsonDocument query,
            bool isMulti)
            : base(requestId)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _query = query;
            _isMulti = isMulti;
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

        public bool IsMulti
        {
            get { return _isMulti; }
        }

        public BsonDocument Query
        {
            get { return _query; }
        }

        // methods
        public new IMessageEncoder<DeleteMessage> GetEncoder(IMessageEncoderFactory encoderFactory)
        {
            return encoderFactory.GetDeleteMessageEncoder();
        }

        protected override IMessageEncoder GetNonGenericEncoder(IMessageEncoderFactory encoderFactory)
        {
            return GetEncoder(encoderFactory);
        }
    }
}
