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
    public class DeleteMessageBinaryEncoder : IMessageEncoder<DeleteMessage>
    {
        #region static
        // static fields
        private static readonly UTF8Encoding __strictEncoding = new UTF8Encoding(false, true);
        #endregion

        // fields
        private readonly Stream _stream;

        // constructors
        public DeleteMessageBinaryEncoder(Stream stream)
        {
            _stream = stream;
        }

        // methods
        private DeleteFlags BuildDeleteFlags(DeleteMessage message)
        {
            var flags = DeleteFlags.None;
            if (!message.IsMulti)
            {
                flags |= DeleteFlags.Single;
            }
            return flags;
        }

        public DeleteMessage ReadMessage()
        {
            var primitiveReader = new BsonPrimitiveReader(_stream);
            var bsonReader = new BsonReader(primitiveReader);

            var messageLength = primitiveReader.ReadInt32();
            var requestId = primitiveReader.ReadInt32();
            var responseTo = primitiveReader.ReadInt32();
            var opcode = (BinaryEncoderOpcode)primitiveReader.ReadInt32();
            var reserved = primitiveReader.ReadInt32();
            var fullCollectionName = primitiveReader.ReadCString();
            var flags = (DeleteFlags)primitiveReader.ReadInt32();
            var query = BsonDocumentSerializer.Instance.Deserialize(bsonReader);

            var firstDot = fullCollectionName.IndexOf('.');
            var databaseName = fullCollectionName.Substring(0, firstDot);
            var collectionName = fullCollectionName.Substring(firstDot + 1);
            var isMulti = !flags.HasFlag(DeleteFlags.Single);

            return new DeleteMessage(
                requestId,
                databaseName,
                collectionName,
                query,
                isMulti);
        }

        public void WriteMessage(DeleteMessage message)
        {
            var startPosition = _stream.Position;
            var primitiveWriter = new BsonPrimitiveWriter(_stream);
            var bsonWriter = new BsonWriter(primitiveWriter);

            primitiveWriter.WriteInt32(0); // messageLength
            primitiveWriter.WriteInt32(message.RequestId);
            primitiveWriter.WriteInt32(0); // responseTo
            primitiveWriter.WriteInt32((int)BinaryEncoderOpcode.Delete);
            primitiveWriter.WriteInt32(0); // reserved
            primitiveWriter.WriteCString(message.DatabaseName + "." + message.CollectionName);
            primitiveWriter.WriteInt32((int)BuildDeleteFlags(message));
            BsonDocumentSerializer.Instance.Serialize(bsonWriter, message.Query ?? new BsonDocument());
            primitiveWriter.BackpatchLength(startPosition);
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

        // nested types
        [Flags]
        private enum DeleteFlags
        {
            None = 0,
            Single = 1
        }
    }
}
