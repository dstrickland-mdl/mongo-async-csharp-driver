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
    public class JsonWriter : IBsonWriter
    {
        // fields
        private Context _context;
        private readonly Stack<Context> _contextStack = new Stack<Context>();
        private readonly TextWriter _writer;

        // constructors
        public JsonWriter(TextWriter writer)
        {
            _writer = writer;
        }

        // methods
        private void AboutToWriteValue()
        {
            if (_context != null)
            {
                switch (_context.Type)
                {
                    case BsonType.Array:
                        if (_context.Index > 0)
                        {
                            _writer.Write(", ");
                        }
                        break;

                    case BsonType.Document:
                        var separator = (_context.Index == 0) ? " " : ", ";
                        _writer.Write("{0}{1} : ", separator, _context.Name);
                        break;
                }
                _context.Index++;
            }
        }

        private void PopContext()
        {
            _context = _contextStack.Pop();
        }

        private void PushContext(BsonType type)
        {
            _contextStack.Push(_context);
            _context = new Context(type);
        }

        public void WriteBinaryData(BsonBinaryData value)
        {
            AboutToWriteValue();
            _writer.Write("BinData()");
        }

        public void WriteBoolean(bool value)
        {
            AboutToWriteValue();
            _writer.Write(value ? "true" : "false");
        }

        public void WriteDateTime(BsonDateTime value)
        {
            AboutToWriteValue();
            _writer.Write(value.ToUniversalTime());
        }

        public void WriteDouble(double value)
        {
            AboutToWriteValue();
            _writer.Write(value);
        }

        public void WriteEndArray()
        {
            _writer.Write("]");
            PopContext();
        }

        public void WriteEndDocument()
        {
            _writer.Write(" }");
            PopContext();
        }

        public void WriteInt32(int value)
        {
            AboutToWriteValue();
            _writer.Write(value);
        }

        public void WriteInt64(long value)
        {
            AboutToWriteValue();
            _writer.Write(value);
        }

        public void WriteJavaScript(BsonJavaScript value)
        {
            AboutToWriteValue();
            _writer.Write("function()");
        }

        public void WriteMaxKey()
        {
            AboutToWriteValue();
            _writer.Write("MaxKey");
        }

        public void WriteMinKey()
        {
            AboutToWriteValue();
            _writer.Write("MinKey");
        }

        public void WriteName(string name)
        {
            if (_context == null | _context.Type != BsonType.Document)
            {
                throw new InvalidOperationException("WriteName is only valid when writing a BsonDocument.");
            }
            _context.Name = name;
        }

        public void WriteNull()
        {
            AboutToWriteValue();
            _writer.Write("null");
        }

        public void WriteObjectId(ObjectId value)
        {
            AboutToWriteValue();
            _writer.Write(string.Format("ObjectId('{0}')", Hex.ToString(value.ToByteArray())));
        }

        public void WriteRegularExpression(BsonRegularExpression value)
        {
            AboutToWriteValue();
            _writer.Write(string.Format("/{0}/{1}", value.Pattern, value.Options));
        }

        public void WriteStartArray()
        {
            AboutToWriteValue();
            _writer.Write("[");
            PushContext(BsonType.Array);
        }

        public void WriteStartDocument()
        {
            AboutToWriteValue();
            _writer.Write("{");
            PushContext(BsonType.Document);
        }

        public void WriteString(string value)
        {
            AboutToWriteValue();
            _writer.Write(string.Format("'{0}'", value.Replace("'", "\\'")));
        }

        public void WriteSymbol(BsonSymbol value)
        {
            AboutToWriteValue();
            _writer.Write(string.Format("symbol('{0}')", value.Name.Replace("'", "\\'")));
        }

        public void WriteTimestamp(BsonTimestamp value)
        {
            AboutToWriteValue();
            _writer.Write(value);
        }

        public void WriteUndefined()
        {
            AboutToWriteValue();
            _writer.Write("undefined");
        }

        // nested classes
        private class Context
        {
            public Context(BsonType type)
            {
                Type = type;
            }

            public int Index;
            public string Name;
            public BsonType Type;
        }
    }
}

