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
    public class BsonPrimitiveWriter
    {
        // fields
        private readonly UTF8Encoding _encoding;
        private readonly Stream _stream;

        // constructors
        public BsonPrimitiveWriter(Stream stream)
            : this(stream, new UTF8Encoding(false, true))
        {
        }

        public BsonPrimitiveWriter(Stream stream, UTF8Encoding encoding)
        {
            _stream = stream;
            _encoding = encoding;
        }

        // properties
        public Stream Stream
        {
            get { return _stream; }
        }

        // methods
        public void BackpatchLength(long startPosition)
        {
            var endPosition = _stream.Position;
            var length = (int)(endPosition - startPosition);
            _stream.Position = startPosition;
            _stream.Write(BitConverter.GetBytes(length), 0, 4);
            _stream.Position = endPosition;
        }

        public void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        public void WriteBytes(byte[] value)
        {
            _stream.Write(value, 0, value.Length);
        }

        public void WriteCString(string value)
        {
            var bytes = _encoding.GetBytes(value);
            if (bytes.Contains<byte>(0))
            {
                throw new ArgumentException("A CString cannot contain null bytes.");
            }
            WriteBytes(bytes);
            WriteByte(0);
        }

        public void WriteDouble(double value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteInt32(int value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteInt64(long value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteObjectId(ObjectId value)
        {
            WriteBytes(value.ToByteArray());
        }

        public void WriteString(string value)
        {
            var bytes = _encoding.GetBytes(value);
            var length = bytes.Length + 1;
            WriteInt32(length);
            WriteBytes(bytes);
            WriteByte(0);
        }
    }
}
