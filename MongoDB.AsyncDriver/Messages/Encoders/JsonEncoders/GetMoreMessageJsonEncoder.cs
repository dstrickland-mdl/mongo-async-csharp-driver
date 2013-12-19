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
    public class GetMoreMessageJsonEncoder : IMessageEncoder<GetMoreMessage>
    {
        // fields
        private readonly JsonWriter _writer;

        // constructors
        public GetMoreMessageJsonEncoder(JsonWriter writer)
        {
            _writer = writer;
        }

        // methods
        public GetMoreMessage ReadMessage()
        {
            throw new NotImplementedException();
        }

        public void WriteMessage(GetMoreMessage message)
        {
            var document = new BsonDocument
            {
                { "opcode", "getMore" },
                { "requestId", message.RequestId },
                { "database", message.DatabaseName },
                { "collection", message.CollectionName },
                { "cursorId", message.CursorId },
                { "batchSize", message.BatchSize }
            };

            BsonDocumentSerializer.Instance.Serialize(_writer, document);
        }

        // explicit interface implementations
        MongoDBMessage IMessageEncoder.ReadMessage()
        {
            return ReadMessage();
        }

        void IMessageEncoder.WriteMessage(MongoDBMessage message)
        {
            WriteMessage((GetMoreMessage)message);
        }
    }
}
