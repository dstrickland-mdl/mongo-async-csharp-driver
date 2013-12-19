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
    public class InsertMessageBinaryEncoder<TDocument> : IMessageEncoder<InsertMessage<TDocument>>
    {
        #region static
        // static fields
        private static readonly UTF8Encoding __strictEncoding = new UTF8Encoding(false, true);
        #endregion

        // fields
        private readonly IBsonSerializer<TDocument> _serializer;
        private readonly Stream _stream;

        // constructors
        public InsertMessageBinaryEncoder(Stream stream, IBsonSerializer<TDocument> serializer)
        {
            _stream = stream;
            _serializer = serializer;
        }

        // methods
        private void AddDocument(BsonPrimitiveWriter primitiveWriter, byte[] document, ProgressTracker tracker)
        {
            primitiveWriter.WriteBytes(document);
            tracker.BatchCount++;
            tracker.BatchLength = (int)_stream.Position - tracker.BatchStartPosition;
        }

        private void AddDocument(BsonWriter bsonWriter, TDocument document, ProgressTracker tracker)
        {
            _serializer.Serialize(bsonWriter, document);
            tracker.BatchCount++;
            tracker.BatchLength = (int)_stream.Position - tracker.BatchStartPosition;
        }

        private InsertFlags BuildInsertFlags(InsertMessage<TDocument> message)
        {
            var flags = InsertFlags.None;
            if (message.ContinueOnError)
            {
                flags |= InsertFlags.ContinueOnError;
            }
            return flags;
        }

        public InsertMessage<TDocument> ReadMessage()
        {
            var startPosition = _stream.Position;
            var primitiveReader = new BsonPrimitiveReader(_stream);
            var bsonReader = new BsonReader(primitiveReader);

            var messageLength = primitiveReader.ReadInt32();
            var requestId = primitiveReader.ReadInt32();
            var responseTo = primitiveReader.ReadInt32();
            var opcode = (BinaryEncoderOpcode)primitiveReader.ReadInt32();
            var flags = (InsertFlags)primitiveReader.ReadInt32();
            var fullCollectionName = primitiveReader.ReadCString();
            var documents = new List<TDocument>();
            while (_stream.Position < startPosition + messageLength)
            {
                var document = _serializer.Deserialize(bsonReader);
                documents.Add(document);
            }

            var firstDot = fullCollectionName.IndexOf('.');
            var databaseName = fullCollectionName.Substring(0, firstDot);
            var collectionName = fullCollectionName.Substring(firstDot + 1);

            var batch = new InsertBatch<TDocument>.FirstBatch(documents.GetEnumerator(), canBeSplit: false);
            var maxBatchCount = 0;
            var maxBatchLength = 0;
            var continueOnError = false;

            return new InsertMessage<TDocument>(
                requestId,
                databaseName,
                collectionName,
                _serializer,
                batch,
                maxBatchCount,
                maxBatchLength,
                continueOnError);
        }

        private byte[] RemoveLastDocument(int documentStartPosition, ProgressTracker tracker)
        {
            var documentLength = (int)_stream.Position - documentStartPosition;
            _stream.Position = documentStartPosition;
            var document = new byte[documentLength];
            _stream.FillBuffer(document, 0, documentLength);
            _stream.Position = documentStartPosition;
            _stream.SetLength(documentStartPosition);
            tracker.BatchCount--;
            tracker.BatchLength = (int)_stream.Position - tracker.BatchStartPosition;
            return document;
        }

        private void WriteDocuments(BsonPrimitiveWriter primitiveWriter, int batchStartPosition, InsertMessage<TDocument> message)
        {
            var bsonWriter = new BsonWriter(primitiveWriter);
            var batch = message.Documents;

            var tracker = new ProgressTracker { BatchStartPosition = batchStartPosition };

            var continuationBatch = batch as ContinuationBatch;
            if (continuationBatch != null)
            {
                AddDocument(primitiveWriter, continuationBatch.Overflow, tracker);
                continuationBatch.Overflow = null; // so it can get garbage collected sooner
            }

            var enumerator = batch.Enumerator;
            while (enumerator.MoveNext())
            {
                var document = enumerator.Current;
                var documentStartPosition = (int)_stream.Position;
                AddDocument(bsonWriter, document, tracker);

                if ((tracker.BatchCount > message.MaxBatchCount || tracker.BatchLength > message.MaxBatchLength) && tracker.BatchCount > 1)
                {
                    var firstBatch = batch as InsertBatch<TDocument>.FirstBatch;
                    if (firstBatch != null && !firstBatch.CanBeSplit)
                    {
                        throw new ArgumentException("The documents did not fit in a single batch.");
                    }

                    var overflow = RemoveLastDocument(documentStartPosition, tracker);
                    var nextBatch = new ContinuationBatch(enumerator, overflow);
                    batch.SetBatchResults(tracker.BatchCount, tracker.BatchLength, null, nextBatch);
                }
            }

            batch.SetBatchResults(tracker.BatchCount, tracker.BatchLength, null, null);
        }

        public void WriteMessage(InsertMessage<TDocument> message)
        {
            var batchStartPosition = (int)_stream.Position;
            var primitiveWriter = new BsonPrimitiveWriter(_stream, __strictEncoding);
            var bsonWriter = new BsonWriter(primitiveWriter);

            primitiveWriter.WriteInt32(0); // messageLength
            primitiveWriter.WriteInt32(message.RequestId);
            primitiveWriter.WriteInt32(0); // responseTo
            primitiveWriter.WriteInt32((int)BinaryEncoderOpcode.Insert);
            primitiveWriter.WriteInt32((int)BuildInsertFlags(message));
            primitiveWriter.WriteCString(message.DatabaseName + "." + message.CollectionName);
            WriteDocuments(primitiveWriter, batchStartPosition, message);
            primitiveWriter.BackpatchLength(batchStartPosition);
        }

        // explicit interface implementations
        MongoDBMessage IMessageEncoder.ReadMessage()
        {
            return ReadMessage();
        }

        void IMessageEncoder.WriteMessage(MongoDBMessage message)
        {
            WriteMessage((InsertMessage<TDocument>)message);
        }

        // nested types
        private class ContinuationBatch : InsertBatch<TDocument>
        {
            // fields
            private byte[] _overflow;

            // constructor
            public ContinuationBatch(IEnumerator<TDocument> enumerator, byte[] overflow)
                : base(enumerator)
            {
                _overflow = overflow;
            }

            // properties
            public byte[] Overflow
            {
                get { return _overflow; }
                set { _overflow = value; }
            }
        }

        [Flags]
        private enum InsertFlags
        {
            None = 0,
            ContinueOnError = 1
        }

        private struct ProgressTracker
        {
            public int BatchStartPosition;
            public int BatchCount;
            public int BatchLength;
        }
    }
}
