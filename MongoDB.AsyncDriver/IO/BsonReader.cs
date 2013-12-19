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
    public class BsonReader : IBsonReader
    {
        #region static
        // static fields
        private static readonly UTF8Encoding __strictEncoding = new UTF8Encoding(false, true);
        #endregion

        // fields
        private Context _context;
        private readonly Stack<Context> _contextStack = new Stack<Context>();
        private readonly BsonPrimitiveReader _primitiveReader;

        // constructors
        public BsonReader(BsonPrimitiveReader primitiveReader)
        {
            _primitiveReader = primitiveReader;
        }

        public BsonReader(Stream stream)
            : this(stream, __strictEncoding)
        {
        }

        public BsonReader(Stream stream, UTF8Encoding encoding)
            : this(new BsonPrimitiveReader(stream, encoding))
        {
        }

        // properties
        public BsonPrimitiveReader PrimitiveReader
        {
            get { return _primitiveReader; }
        }

        // methods
        private void AboutToReadValue()
        {
            if (_context != null && _context.Type == BsonType.Array)
            {
                _primitiveReader.SkipCString();
            }
        }

        private void PopContext()
        {
            var length = _primitiveReader.Stream.Position - _context.StartPosition;
            if (length != _context.ExpectedLength)
            {
                var message = string.Format("Length is {0} but expected {1}.", length, _context.ExpectedLength);
                throw new FormatException(message);
            }
            _context = _contextStack.Pop();
        }

        private void PushContext(BsonType type)
        {
            _contextStack.Push(_context);
            var startPosition = _primitiveReader.Stream.Position;
            var expectedLength = _primitiveReader.ReadInt32();
            _context = new Context(type, startPosition, expectedLength);
        }

        public BsonBinaryData ReadBinaryData()
        {
            AboutToReadValue();
            var length = _primitiveReader.ReadInt32();
            var subType = (BinarySubType)_primitiveReader.ReadByte();
#pragma warning disable 618
            if (subType == BinarySubType.ObsoleteBinary)
#pragma warning restore
            {
                var innerLength = _primitiveReader.ReadInt32();
                if (innerLength != length - 5)
                {
                    throw new FormatException("The two length fields of a binary value of sub type ObsoleteBinary don't agree.");
                }
                var bytes = _primitiveReader.ReadBytes(innerLength);
                return new BsonBinaryData(bytes, subType);
            }
            else
            {
                var bytes = _primitiveReader.ReadBytes(length);
                return new BsonBinaryData(bytes, subType);
            }
        }

        public bool ReadBoolean()
        {
            AboutToReadValue();
            return _primitiveReader.ReadByte() != 0;
        }

        public BsonDateTime ReadDateTime()
        {
            AboutToReadValue();
            var millisecondsSinceEpoch = _primitiveReader.ReadInt64();
            return new BsonDateTime(millisecondsSinceEpoch);
        }

        public double ReadDouble()
        {
            AboutToReadValue();
            return _primitiveReader.ReadDouble();
        }

        public void ReadEndArray()
        {
            PopContext();
        }

        public void ReadEndDocument()
        {
            PopContext();
        }

        public int ReadInt32()
        {
            AboutToReadValue();
            return _primitiveReader.ReadInt32();
        }

        public long ReadInt64()
        {
            AboutToReadValue();
            return _primitiveReader.ReadInt64();
        }

        public BsonJavaScript ReadJavaScript(BsonType type)
        {
            AboutToReadValue();
            string code;
            switch (type)
            {
                case BsonType.JavaScript:
                    code = _primitiveReader.ReadString();
                    return new BsonJavaScript(code);

                case BsonType.JavaScriptWithScope:
                    PushContext(BsonType.JavaScriptWithScope);
                    code = _primitiveReader.ReadString();
                    var scope = BsonDocumentSerializer.Instance.Deserialize(this);
                    PopContext();
                    return new BsonJavaScript(code, scope);

                default:
                    throw new ArgumentException("Invalid BsonType.");
            }
        }

        public BsonMaxKey ReadMaxKey()
        {
            AboutToReadValue();
            return BsonMaxKey.Instance;
        }

        public BsonMinKey ReadMinKey()
        {
            AboutToReadValue();
            return BsonMinKey.Instance;
        }

        public string ReadName()
        {
            AboutToReadValue();
            return _primitiveReader.ReadCString();
        }

        public BsonNull ReadNull()
        {
            AboutToReadValue();
            return BsonNull.Instance;
        }

        public ObjectId ReadObjectId()
        {
            AboutToReadValue();
            return _primitiveReader.ReadObjectId();
        }

        public BsonRegularExpression ReadRegularExpression()
        {
            AboutToReadValue();
            var pattern = _primitiveReader.ReadCString();
            var options = _primitiveReader.ReadCString();
            return new BsonRegularExpression(pattern, options);
        }

        public void ReadStartArray()
        {
            AboutToReadValue();
            PushContext(BsonType.Array);
        }

        public void ReadStartDocument()
        {
            AboutToReadValue();
            PushContext(BsonType.Document);
        }

        public string ReadString()
        {
            AboutToReadValue();
            return _primitiveReader.ReadString();
        }

        public BsonSymbol ReadSymbol()
        {
            AboutToReadValue();
            var name = _primitiveReader.ReadString();
            return new BsonSymbol(name);
        }

        public BsonTimestamp ReadTimestamp()
        {
            AboutToReadValue();
            var increment = _primitiveReader.ReadInt32();
            var timestamp = _primitiveReader.ReadInt32();
            return new BsonTimestamp(timestamp, increment);
        }

        public BsonType ReadType()
        {
            return (BsonType)_primitiveReader.ReadByte();
        }

        public BsonUndefined ReadUndefined()
        {
            AboutToReadValue();
            return BsonUndefined.Instance;
        }

        public void SkipValue(BsonType type)
        {
            int length;
            switch (type)
            {
                case BsonType.MaxKey:
                case BsonType.MinKey:
                case BsonType.Null:
                case BsonType.Undefined:
                    length = 0;
                    break;

                case BsonType.Boolean:
                    length = 1;
                    break;

                case BsonType.Int32:
                    length = 4;
                    break;

                case BsonType.DateTime:
                case BsonType.Double:
                case BsonType.Int64:
                case BsonType.Timestamp:
                    length = 8;
                    break;

                case BsonType.ObjectId:
                    length = 12;
                    break;

                case BsonType.Array:
                case BsonType.Binary:
                case BsonType.Document:
                case BsonType.JavaScriptWithScope:
                    length = _primitiveReader.ReadInt32();
                    break;

                case BsonType.JavaScript:
                case BsonType.String:
                case BsonType.Symbol:
                    length = _primitiveReader.ReadInt32() + 4;
                    break;

                case BsonType.RegularExpression:
                    _primitiveReader.SkipCString();
                    _primitiveReader.SkipCString();
                    length = 0;
                    break;

                default:
                    throw new ArgumentException("Invalid BsonType");
            }

            _primitiveReader.Stream.Seek(length, SeekOrigin.Current);
        }

        // nested classes
        private class Context
        {
            public Context(BsonType type, long position, int expectedLength)
            {
                Type = type;
                StartPosition = position;
                ExpectedLength = expectedLength;
            }

            public BsonType Type;
            public long StartPosition;
            public int ExpectedLength;
        }
    }
}
