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
    public class BsonPrimitiveReader
    {
        // fields
        private readonly byte[] _buffer = new byte[32];
        private readonly UTF8Encoding _encoding;
        private readonly Stream _stream;

        // constructors
        public BsonPrimitiveReader(Stream stream)
            : this(stream, new UTF8Encoding(false, true))
        {
        }

        public BsonPrimitiveReader(Stream stream, UTF8Encoding encoding)
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
        public byte ReadByte()
        {
            var b = _stream.ReadByte();
            if (b == -1)
            {
                throw new EndOfStreamException();
            }
            return (byte)b;
        }

        public byte[] ReadBytes(int count)
        {
            var bytes = new byte[count];
            _stream.FillBuffer(bytes, 0, count);
            return bytes;
        }

        public string ReadCString()
        {
            var memoryStream = new MemoryStream(32); // override default capacity of zero
            while (true)
            {
                var b = ReadByte();
                if (b == 0)
                {
                    break;
                }
                else
                {
                    memoryStream.WriteByte(b);
                }
            }
            return _encoding.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        public double ReadDouble()
        {
            _stream.FillBuffer(_buffer, 0, 8);
            return BitConverter.ToDouble(_buffer, 0);
        }

        public int ReadInt32()
        {
            _stream.FillBuffer(_buffer, 0, 4);
            return BitConverter.ToInt32(_buffer, 0);
        }

        public long ReadInt64()
        {
            _stream.FillBuffer(_buffer, 0, 8);
            return BitConverter.ToInt64(_buffer, 0);
        }

        public void ReadNullByte()
        {
            var b = ReadByte();
            if (b != 0)
            {
                throw new FormatException("Missing null byte.");
            }
        }

        public ObjectId ReadObjectId()
        {
            _stream.FillBuffer(_buffer, 0, 12);
            return new ObjectId(_buffer, 0);
        }

        public string ReadString()
        {
            var length = ReadInt32();
            var bytes = ReadBytes(length - 1);
            ReadNullByte();
            return _encoding.GetString(bytes);
        }

        public void SkipCString()
        {
            while (ReadByte() != 0) { }
        }
    }
}
