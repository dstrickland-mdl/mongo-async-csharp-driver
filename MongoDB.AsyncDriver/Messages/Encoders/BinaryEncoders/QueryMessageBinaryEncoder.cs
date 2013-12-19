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
    public class QueryMessageBinaryEncoder : IMessageEncoder<QueryMessage>
    {
        #region static
        // static fields
        private static readonly UTF8Encoding __strictEncoding = new UTF8Encoding(false, true);
        #endregion

        // fields
        private readonly Stream _stream;

        // constructors
        public QueryMessageBinaryEncoder(Stream stream)
        {
            _stream = stream;
        }

        // methods
        private QueryFlags BuildQueryFlags(QueryMessage message)
        {
            var flags = QueryFlags.None;
            if (message.NoCursorTimeout)
            {
                flags |= QueryFlags.NoCursorTimeout;
            }
            if (message.PartialOk)
            {
                flags |= QueryFlags.Partial;
            }
            if (message.SlaveOk)
            {
                flags |= QueryFlags.SlaveOk;
            }
            return flags;
        }

        public QueryMessage ReadMessage()
        {
            var startPosition = _stream.Position;
            var primitiveReader = new BsonPrimitiveReader(_stream, __strictEncoding);
            var bsonReader = new BsonReader(primitiveReader);

            var messageLength = primitiveReader.ReadInt32();
            var requestId = primitiveReader.ReadInt32();
            var responseTo = primitiveReader.ReadInt32();
            var opcode = (BinaryEncoderOpcode)primitiveReader.ReadInt32();
            var flags = (QueryFlags)primitiveReader.ReadInt32();
            var fullCollectionName = primitiveReader.ReadCString();
            var skip = primitiveReader.ReadInt32();
            var batchSize = primitiveReader.ReadInt32();
            var query = BsonDocumentSerializer.Instance.Deserialize(bsonReader);
            BsonDocument fields = null;
            if (_stream.Position < startPosition + messageLength)
            {
                fields = BsonDocumentSerializer.Instance.Deserialize(bsonReader);
            }

            var firstDot = fullCollectionName.IndexOf('.');
            var databaseName = fullCollectionName.Substring(0, firstDot);
            var collectionName = fullCollectionName.Substring(firstDot + 1);
            var slaveOk = flags.HasFlag(QueryFlags.SlaveOk);
            var partialOk = flags.HasFlag(QueryFlags.Partial);
            var noCursorTimeout = flags.HasFlag(QueryFlags.NoCursorTimeout);

            return new QueryMessage(
                requestId,
                databaseName,
                collectionName,
                query,
                fields,
                skip,
                batchSize,
                slaveOk,
                partialOk,
                noCursorTimeout);
        }

        public void WriteMessage(QueryMessage message)
        {
            var startPosition = _stream.Position;
            var primitiveWriter = new BsonPrimitiveWriter(_stream, __strictEncoding);
            var bsonWriter = new BsonWriter(primitiveWriter);

            primitiveWriter.WriteInt32(0); // messageLength
            primitiveWriter.WriteInt32(message.RequestId);
            primitiveWriter.WriteInt32(0); // responseTo
            primitiveWriter.WriteInt32((int)BinaryEncoderOpcode.Query);
            primitiveWriter.WriteInt32((int)BuildQueryFlags(message));
            primitiveWriter.WriteCString(message.DatabaseName + "." + message.CollectionName);
            primitiveWriter.WriteInt32(message.Skip);
            primitiveWriter.WriteInt32(message.BatchSize);
            BsonDocumentSerializer.Instance.Serialize(bsonWriter, message.Query ?? new BsonDocument());
            if (message.Fields != null)
            {
                BsonDocumentSerializer.Instance.Serialize(bsonWriter, message.Fields);
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
            WriteMessage((QueryMessage)message);
        }

        // nested types
        [Flags]
        private enum QueryFlags
        {
            None = 0,
            TailableCursor = 2,
            SlaveOk = 4,
            NoCursorTimeout = 16,
            AwaitData = 32,
            Exhaust = 64,
            Partial = 128
        }
    }
}
