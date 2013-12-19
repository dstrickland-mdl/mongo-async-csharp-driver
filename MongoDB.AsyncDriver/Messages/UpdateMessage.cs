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
    public class UpdateMessage : RequestMessage
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly bool _isMulti;
        private readonly bool _isUpsert;
        private readonly BsonDocument _query;
        private readonly BsonDocument _update;

        // constructors
        public UpdateMessage(
            int requestId,
            string databaseName,
            string collectionName,
            BsonDocument query,
            BsonDocument update,
            bool isMulti,
            bool isUpsert)
            : base(requestId)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _query = query;
            _update = update;
            _isMulti = isMulti;
            _isUpsert = isUpsert;
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

        public bool IsUpsert
        {
            get { return _isUpsert; }
        }

        public BsonDocument Query
        {
            get { return _query; }
        }

        public BsonDocument Update
        {
            get { return _update; }
        }

        // methods
        public new IMessageEncoder<UpdateMessage> GetEncoder(IMessageEncoderFactory encoderFactory)
        {
            return encoderFactory.GetUpdateMessageEncoder();
        }

        protected override IMessageEncoder GetNonGenericEncoder(IMessageEncoderFactory encoderFactory)
        {
            return GetEncoder(encoderFactory);
        }
    }
}
