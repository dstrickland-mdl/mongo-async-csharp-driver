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
    public class ReplyMessageBinaryEncoder<TDocument> : IMessageEncoder<ReplyMessage<TDocument>>
    {
        // fields
        private readonly IBsonSerializer<TDocument> _serializer;
        private readonly BsonReader _reader;

        // constructors
        public ReplyMessageBinaryEncoder(Stream stream, IBsonSerializer<TDocument> serializer)
        {
            _reader = new BsonReader(stream);
            _serializer = serializer;
        }

        // methods
        public ReplyMessage<TDocument> ReadMessage()
        {
            var messageLength = _reader.ReadInt32();
            var requestId = _reader.ReadInt32();
            var responseTo = _reader.ReadInt32();
            var opcode = (BinaryEncoderOpcode)_reader.ReadInt32();
            var flags = (ResponseFlags)_reader.ReadInt32();
            var cursorId = _reader.ReadInt64();
            var startingFrom = _reader.ReadInt32();
            var numberReturned = _reader.ReadInt32();
            List<TDocument> documents = null;
            BsonDocument queryFailureDocument = null;

            var cursorNotFound = flags.HasFlag(ResponseFlags.CursorNotFound);
            var queryFailure = flags.HasFlag(ResponseFlags.QueryFailure);

            if (queryFailure)
            {
                queryFailureDocument = BsonDocumentSerializer.Instance.Deserialize(_reader);
            }
            else
            {
                documents = new List<TDocument>();
                for (var i = 0; i < numberReturned; i++)
                {
                    documents.Add(_serializer.Deserialize(_reader));
                }
            }

            return new ReplyMessage<TDocument>(
                cursorId,
                cursorNotFound,
                documents,
                numberReturned,
                queryFailure,
                queryFailureDocument,
                requestId,
                responseTo,
                _serializer,
                startingFrom);
        }

        public void WriteMessage(ReplyMessage<TDocument> message)
        {
            throw new NotImplementedException();
        }

        // explicit interface implementations
        MongoDBMessage IMessageEncoder.ReadMessage()
        {
            return ReadMessage();
        }

        void IMessageEncoder.WriteMessage(MongoDBMessage message)
        {
            WriteMessage((ReplyMessage<TDocument>)message);
        }

        // nested types
        [Flags]
        internal enum ResponseFlags
        {
            CursorNotFound = 1,
            QueryFailure = 2,
            AwaitCapable = 8
        }
    }
}
