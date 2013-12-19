﻿/* Copyright 2013-2014 MongoDB Inc.
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
    public class DeleteMessageJsonEncoder : IMessageEncoder<DeleteMessage>
    {
        // fields
        private readonly JsonWriter _writer;

        // constructors
        public DeleteMessageJsonEncoder(JsonWriter writer)
        {
            _writer = writer;
        }

        // methods
        public DeleteMessage ReadMessage()
        {
            throw new NotImplementedException();
        }

        public void WriteMessage(DeleteMessage message)
        {
            var document = new BsonDocument
            {
                { "opcode", "delete" },
                { "requestId", message.RequestId },
                { "database", message.DatabaseName },
                { "collection", message.CollectionName },
                { "query", message.Query ?? new BsonDocument() },
                { "isMulti", message.IsMulti }
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
            WriteMessage((DeleteMessage)message);
        }
    }
}
