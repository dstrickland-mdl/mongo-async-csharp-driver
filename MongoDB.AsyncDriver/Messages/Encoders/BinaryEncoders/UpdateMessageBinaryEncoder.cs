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
    public class UpdateMessageBinaryEncoder : IMessageEncoder<UpdateMessage>
    {
        #region static
        // static fields
        private static readonly UTF8Encoding __strictEncoding = new UTF8Encoding(false, true);
        #endregion

        // fields
        private readonly Stream _stream;

        // constructors
        public UpdateMessageBinaryEncoder(Stream stream)
        {
            _stream = stream;
        }

        // methods
        private UpdateFlags BuildUpdateFlags(UpdateMessage message)
        {
            var flags = UpdateFlags.None;
            if (message.IsMulti)
            {
                flags |= UpdateFlags.Multi;
            }
            if (message.IsUpsert)
            {
                flags |= UpdateFlags.Upsert;
            }
            return flags;
        }

        public UpdateMessage ReadMessage()
        {
            var primitiveReader = new BsonPrimitiveReader(_stream, __strictEncoding);
            var bsonReader = new BsonReader(primitiveReader);

            var messageLength = primitiveReader.ReadInt32();
            var requestId = primitiveReader.ReadInt32();
            var responseTo = primitiveReader.ReadInt32();
            var opcode = (BinaryEncoderOpcode)primitiveReader.ReadInt32();
            var reserved = primitiveReader.ReadInt32();
            var fullCollectionName = primitiveReader.ReadCString();
            var flags = (UpdateFlags)primitiveReader.ReadInt32();
            var query = BsonDocumentSerializer.Instance.Deserialize(bsonReader);
            var update = BsonDocumentSerializer.Instance.Deserialize(bsonReader);

            var firstDot = fullCollectionName.IndexOf('.');
            var databaseName = fullCollectionName.Substring(0, firstDot);
            var collectionName = fullCollectionName.Substring(firstDot + 1);
            var isMulti = flags.HasFlag(UpdateFlags.Multi);
            var isUpsert = flags.HasFlag(UpdateFlags.Upsert);

            return new UpdateMessage(
                requestId,
                databaseName,
                collectionName,
                query,
                update,
                isMulti,
                isUpsert);
        }

        public void WriteMessage(UpdateMessage message)
        {
            var startPosition = _stream.Position;
            var primitiveWriter = new BsonPrimitiveWriter(_stream, __strictEncoding);
            var bsonWriter = new BsonWriter(primitiveWriter);

            primitiveWriter.WriteInt32(0); // messageLength
            primitiveWriter.WriteInt32(message.RequestId);
            primitiveWriter.WriteInt32(0); // responseTo
            primitiveWriter.WriteInt32((int)BinaryEncoderOpcode.Update);
            primitiveWriter.WriteInt32(0); // reserved
            primitiveWriter.WriteCString(message.DatabaseName + "." + message.CollectionName);
            primitiveWriter.WriteInt32((int)BuildUpdateFlags(message));
            BsonDocumentSerializer.Instance.Serialize(bsonWriter, message.Query ?? new BsonDocument());
            BsonDocumentSerializer.Instance.Serialize(bsonWriter, message.Update ?? new BsonDocument());
            primitiveWriter.BackpatchLength(startPosition);
        }

        // explicit interface implementations
        MongoDBMessage IMessageEncoder.ReadMessage()
        {
            return ReadMessage();
        }

        void IMessageEncoder.WriteMessage(MongoDBMessage message)
        {
            WriteMessage((UpdateMessage)message);
        }

        // nested types
        [Flags]
        public enum UpdateFlags
        {
            None = 0,
            Upsert = 1,
            Multi = 2
        }
    }
}
