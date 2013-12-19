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
    public class KillCursorsMessageBinaryEncoder : IMessageEncoder<KillCursorsMessage>
    {
        #region static
        // static fields
        private static readonly UTF8Encoding __strictEncoding = new UTF8Encoding(false, true);
        #endregion

        // fields
        private readonly Stream _stream;

        // constructors
        public KillCursorsMessageBinaryEncoder(Stream stream)
        {
            _stream = stream;
        }

        // methods
        public KillCursorsMessage ReadMessage()
        {
            var primitiveReader = new BsonPrimitiveReader(_stream, __strictEncoding);

            var messageLength = primitiveReader.ReadInt32();
            var requestId = primitiveReader.ReadInt32();
            var responseTo = primitiveReader.ReadInt32();
            var opcode = (BinaryEncoderOpcode)primitiveReader.ReadInt32();
            var reserved = primitiveReader.ReadInt32();
            var count = primitiveReader.ReadInt32();
            var cursorIds = new long[count];
            for (var i = 0; i < count; i++)
            {
                cursorIds[i] = primitiveReader.ReadInt64();
            }

            return new KillCursorsMessage(
                requestId,
                cursorIds);
        }

        public void WriteMessage(KillCursorsMessage message)
        {
            var startPosition = _stream.Position;
            var primitiveWriter = new BsonPrimitiveWriter(_stream, __strictEncoding);

            primitiveWriter.WriteInt32(0); // messageLength
            primitiveWriter.WriteInt32(message.RequestId);
            primitiveWriter.WriteInt32(0); // responseTo
            primitiveWriter.WriteInt32((int)BinaryEncoderOpcode.KillCursors);
            primitiveWriter.WriteInt32(0); // reserved
            primitiveWriter.WriteInt32(message.CursorIds.Count);
            foreach (var cursorId in message.CursorIds)
            {
                primitiveWriter.WriteInt64(cursorId);
            }
            primitiveWriter.BackpatchLength(startPosition);
        }

        // explicit interface implementations
        MongoDBMessage IMessageEncoder.ReadMessage()
        {
            return ReadMessage();
        }

        void IMessageEncoder.WriteMessage(MongoDBMessage message)
        {
            WriteMessage((KillCursorsMessage)message);
        }
    }
}
