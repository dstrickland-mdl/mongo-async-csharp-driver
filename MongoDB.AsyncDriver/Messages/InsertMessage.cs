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
    public class InsertMessage<TDocument> : RequestMessage, IEncodableMessage<InsertMessage<TDocument>>
    {
        // fields
        private readonly string _collectionName;
        private readonly bool _continueOnError;
        private readonly string _databaseName;
        private readonly Batch<TDocument> _documents;
        private readonly int _maxBatchCount;
        private readonly int _maxBatchLength;
        private readonly IBsonSerializer<TDocument> _serializer;

        // constructors
        public InsertMessage(
            int requestId,
            string databaseName,
            string collectionName,
            IBsonSerializer<TDocument> serializer,
            Batch<TDocument> documents,
            int maxBatchCount,
            int maxBatchLength,
            bool continueOnError)
            : base(requestId)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _serializer = serializer;
            _documents = documents;
            _maxBatchCount = maxBatchCount;
            _maxBatchLength = maxBatchLength;
            _continueOnError = continueOnError;
        }

        // properties
        public string CollectionName
        {
            get { return _collectionName; }
        }

        public bool ContinueOnError
        {
            get { return _continueOnError; }
        }

        public string DatabaseName
        {
            get { return _databaseName; }
        }

        public Batch<TDocument> Documents
        {
            get { return _documents; }
        }

        public int MaxBatchCount
        {
            get { return _maxBatchCount; }
        }

        public int MaxBatchLength
        {
            get { return _maxBatchLength; }
        }

        public IBsonSerializer<TDocument> Serializer
        {
            get { return _serializer; }
        }

        // methods
        public new IMessageEncoder<InsertMessage<TDocument>> GetEncoder(IMessageEncoderFactory encoderFactory)
        {
            return encoderFactory.GetInsertMessageEncoder<TDocument>(_serializer);
        }

        protected override IMessageEncoder GetNonGenericEncoder(IMessageEncoderFactory encoderFactory)
        {
            return GetEncoder(encoderFactory);
        }
    }
}
