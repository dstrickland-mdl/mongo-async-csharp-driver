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
    public class JsonMessageEncoderFactory : IMessageEncoderFactory
    {
        // fields
        private readonly JsonWriter _writer;

        // constructors
        public JsonMessageEncoderFactory(JsonWriter writer)
        {
            _writer = writer;
        }

        // methods
        public IMessageEncoder<DeleteMessage> GetDeleteMessageEncoder()
        {
            return new DeleteMessageJsonEncoder(_writer);
        }

        public IMessageEncoder<GetMoreMessage> GetGetMoreMessageEncoder()
        {
            return new GetMoreMessageJsonEncoder(_writer);
        }

        public IMessageEncoder<InsertMessage<TDocument>> GetInsertMessageEncoder<TDocument>(IBsonSerializer<TDocument> serializer)
        {
            return new InsertMessageJsonEncoder<TDocument>(_writer, serializer);
        }

        public IMessageEncoder<KillCursorsMessage> GetKillCursorsMessageEncoder()
        {
            return new KillCursorsMessageJsonEncoder(_writer);
        }

        public IMessageEncoder<QueryMessage> GetQueryMessageEncoder()
        {
            return new QueryMessageJsonEncoder(_writer);
        }

        public IMessageEncoder<ReplyMessage<TDocument>> GetReplyMessageEncoder<TDocument>(IBsonSerializer<TDocument> serializer)
        {
            return new ReplyMessageJsonEncoder<TDocument>(_writer, serializer);
        }

        public IMessageEncoder<UpdateMessage> GetUpdateMessageEncoder()
        {
            return new UpdateMessageJsonEncoder(_writer);
        }
    }
}
