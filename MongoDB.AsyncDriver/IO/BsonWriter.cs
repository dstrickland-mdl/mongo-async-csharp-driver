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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public class BsonWriter : IBsonWriter
    {
        #region static
        // static fields
        private static readonly UTF8Encoding __strictEncoding = new UTF8Encoding(false, true);
        #endregion

        // fields
        private Context _context;
        private readonly Stack<Context> _contextStack = new Stack<Context>();
        private readonly BsonPrimitiveWriter _primitiveWriter;

        // constructors
        public BsonWriter(BsonPrimitiveWriter primitiveWriter)
        {
            _primitiveWriter = primitiveWriter;
        }

        public BsonWriter(Stream stream)
            : this(stream, __strictEncoding)
        {
        }

        public BsonWriter(Stream stream, UTF8Encoding encoding)
            : this(new BsonPrimitiveWriter(stream, encoding))
        {
        }

        // properties
        public BsonPrimitiveWriter PrimitiveWriter
        {
            get { return _primitiveWriter; }
        }

        // methods
        private void AboutToWriteValue(BsonType type)
        {
            if (_context != null)
            {
                switch (_context.Type)
                {
                    case BsonType.Array:
                        var indexString = (_context.Index++).ToString(NumberFormatInfo.InvariantInfo);
                        _primitiveWriter.WriteByte((byte)type);
                        _primitiveWriter.WriteCString(indexString);
                        return;

                    case BsonType.Document:
                        if (_context.Name == null)
                        {
                            throw new InvalidOperationException("Need to call WriteName first.");
                        }
                        _primitiveWriter.WriteByte((byte)type);
                        _primitiveWriter.WriteCString(_context.Name);
                        _context.Name = null;
                        return;
                }
            }
        }

        private void PopContext()
        {
            _primitiveWriter.BackpatchLength(_context.StartPosition);
            _context = _contextStack.Pop();
        }

        private void PushContext(BsonType type)
        {
            _contextStack.Push(_context);
            _context = new Context(type, _primitiveWriter.Stream.Position);
            _primitiveWriter.WriteInt32(0); // will be backpatched when PopContext is called
        }

        public void WriteBinaryData(BsonBinaryData value)
        {
            AboutToWriteValue(BsonType.Binary);
#pragma warning disable 618
            if (value.SubType == BinarySubType.ObsoleteBinary)
#pragma warning restore
            {
                _primitiveWriter.WriteInt32(value.Bytes.Length + 4);
                _primitiveWriter.WriteByte((byte)value.SubType);
                _primitiveWriter.WriteInt32(value.Bytes.Length);
                _primitiveWriter.WriteBytes(value.Bytes);
            }
            else
            {
                _primitiveWriter.WriteInt32(value.Bytes.Length);
                _primitiveWriter.WriteByte((byte)value.SubType);
                _primitiveWriter.WriteBytes(value.Bytes);
            }
        }

        public void WriteBoolean(bool value)
        {
            AboutToWriteValue(BsonType.Boolean);
            _primitiveWriter.WriteByte((byte)(value ? 1 : 0));
        }

        public void WriteDateTime(BsonDateTime value)
        {
            AboutToWriteValue(BsonType.DateTime);
            _primitiveWriter.WriteInt64(value.MillisecondsSinceEpoch);
        }

        public void WriteDouble(double value)
        {
            AboutToWriteValue(BsonType.Double);
            _primitiveWriter.WriteDouble(value);
        }

        public void WriteEndArray()
        {
            _primitiveWriter.WriteByte(0);
            PopContext();
        }

        public void WriteEndDocument()
        {
            _primitiveWriter.WriteByte(0);
            PopContext();
        }

        public void WriteInt32(int value)
        {
            AboutToWriteValue(BsonType.Int32);
            _primitiveWriter.WriteInt32(value);
        }

        public void WriteInt64(long value)
        {
            AboutToWriteValue(BsonType.Int64);
            _primitiveWriter.WriteInt64(value);
        }

        public void WriteJavaScript(BsonJavaScript value)
        {
            AboutToWriteValue(value.Type);
            if (value.Type == BsonType.JavaScriptWithScope)
            {
                PushContext(BsonType.JavaScriptWithScope);
                _primitiveWriter.WriteString(value.Code);
                BsonDocumentSerializer.Instance.Serialize(this, value.Scope);
                PopContext();
            }
            else
            {
                _primitiveWriter.WriteString(value.Code);
            }
        }

        public void WriteMaxKey()
        {
            AboutToWriteValue(BsonType.MaxKey);
        }

        public void WriteMinKey()
        {
            AboutToWriteValue(BsonType.MinKey);
        }

        public void WriteName(string name)
        {
            if (_context == null || _context.Type != BsonType.Document)
            {
                throw new InvalidOperationException("WriteName is only valid when writing a BsonDocument.");
            }
            _context.Name = name;
        }

        public void WriteNull()
        {
            AboutToWriteValue(BsonType.Null);
        }

        public void WriteObjectId(ObjectId value)
        {
            AboutToWriteValue(BsonType.ObjectId);
            _primitiveWriter.WriteObjectId(value);
        }

        public void WriteRegularExpression(BsonRegularExpression value)
        {
            AboutToWriteValue(BsonType.RegularExpression);
            _primitiveWriter.WriteCString(value.Pattern);
            _primitiveWriter.WriteCString(value.Options);
        }

        public void WriteStartArray()
        {
            AboutToWriteValue(BsonType.Array);
            PushContext(BsonType.Array);
        }

        public void WriteStartDocument()
        {
            AboutToWriteValue(BsonType.Document);
            PushContext(BsonType.Document);
        }

        public void WriteString(string value)
        {
            AboutToWriteValue(BsonType.String);
            _primitiveWriter.WriteString(value);
        }

        public void WriteSymbol(BsonSymbol value)
        {
            AboutToWriteValue(BsonType.Symbol);
            _primitiveWriter.WriteString(value.Name);
        }

        public void WriteTimestamp(BsonTimestamp value)
        {
            AboutToWriteValue(BsonType.Timestamp);
            _primitiveWriter.WriteInt32(value.Increment);
            _primitiveWriter.WriteInt32(value.Timestamp);
        }

        public void WriteUndefined()
        {
            AboutToWriteValue(BsonType.Undefined);
        }

        // nested classes
        private class Context
        {
            public Context(BsonType type, long position)
            {
                Type = type;
                StartPosition = position;
            }

            public int Index;
            public string Name;
            public long StartPosition;
            public BsonType Type;
        }
    }
}
