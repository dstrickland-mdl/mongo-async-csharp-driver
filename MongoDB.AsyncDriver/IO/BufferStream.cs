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
    public class BufferStream : Stream
    {
        // fields
        private IBuffer _buffer;
        private readonly IBufferPool _bufferPool;
        private int _capacity;
        private bool _disposed;
        private int _length;
        private int _position;

        // constructors
        public BufferStream(IBufferPool bufferPool, int capacity)
        {
            if (bufferPool == null)
            {
                throw new ArgumentNullException("bufferPool");
            }
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", "Capacity is negative.");
            }

            _bufferPool = bufferPool;
            _buffer = bufferPool.Acquire(capacity);
            _capacity = _buffer.Bytes.Length;
        }

        // properties
        public IBuffer Buffer
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("BufferStream");
                }

                _buffer.Length = _length;
                return _buffer;
            }
        }

        public override bool CanRead
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("BufferStream");
                }

                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("BufferStream");
                }

                return true;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("BufferStream");
                }

                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("BufferStream");
                }

                return true;
            }
        }

        public override long Length
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("BufferStream");
                }

                return _length;
            }
        }

        public override long Position
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("BufferStream");
                }

                return _position;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Value is negative.");
                }
                if (value > int.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("value", "Value is greater than int.MaxValue.");
                }
                if (_disposed)
                {
                    throw new ObjectDisposedException("BufferStream");
                }

                _position = (int)value;
            }
        }

        // methods
        private void AdvancePositionAfterWrite(int count)
        {
            _position += count;
            if (_length < _position)
            {
                _length = _position;
            }
        }

        public IBuffer DetachBuffer()
        {
            _buffer.Length = _length;
            var buffer = _buffer;
            _buffer = null;
            return buffer;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (_buffer != null)
                {
                    _buffer.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        private void EnsureCapacity(int capacity)
        {
            if (capacity > _capacity)
            {
                var newBuffer = _bufferPool.Acquire(capacity);
                System.Buffer.BlockCopy(_buffer.Bytes, 0, newBuffer.Bytes, 0, _length);
                _buffer.Dispose();
                _buffer = newBuffer;
                _capacity = newBuffer.Bytes.Length;
            }
        }

        public override void Flush()
        {
            // do nothing
        }

        private void PrepareToWrite(int count)
        {
            if ((long)_position + (long)count > int.MaxValue)
            {
                throw new IOException("Capacity is limited to Int32.MaxValue.");
            }

            var newPosition = _position + count;
            EnsureCapacity(newPosition);

            if (_position > _length)
            {
                Array.Clear(_buffer.Bytes, _position, newPosition - _length);
            }
        }

        public override int Read(byte[] destination, int offset, int count)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Offset is negative.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count is negative.");
            }
            if (offset + count > destination.Length)
            {
                throw new ArgumentException("Count extends beyond the end of the destination buffer.", "count");
            }
            if (_disposed)
            {
                throw new ObjectDisposedException("BufferStream");
            }

            var available = _length - _position;
            if (count > available)
            {
                if (available <= 0)
                {
                    return 0;
                }
                count = available;
            }

            System.Buffer.BlockCopy(_buffer.Bytes, _position, destination, offset, count);
            _position += count;
            return count;
        }

        public override int ReadByte()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("BufferStream");
            }
            if (_position >= _length)
            {
                return -1;
            }
            return _buffer.Bytes[_position++];
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("BufferStream");
            }

            long position;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = offset;
                    break;
                case SeekOrigin.Current:
                    position = _position + offset;
                    break;
                case SeekOrigin.End:
                    position = _length + offset;
                    break;
                default:
                    throw new ArgumentException(string.Format("Invalid origin: {0}.", origin));
            }

            if (position < 0)
            {
                throw new IOException("Seek attempted to negative position.");
            }
            if (position > int.MaxValue)
            {
                throw new IOException("Seek attempted beyond int.MaxValue.");
            }

            _position = (int)position;
            return position;
        }

        public override void SetLength(long value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("value", "Value is negative.");
            }
            if (value > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("value", "Value is greater than int.MaxValue.");
            }
            if (_disposed)
            {
                throw new ObjectDisposedException("BufferStream");
            }

            EnsureCapacity((int)value);
            _length = (int)value;
        }

        public override void Write(byte[] source, int offset, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (offset < 0)
            {
                throw new ArgumentException("Offset is negative.", "offset");
            }
            if (count < 0)
            {
                throw new ArgumentException("Count is negative.", "count");
            }
            if (offset + count > source.Length)
            {
                throw new ArgumentException("Count extends beyond the end of the source buffer.", "count");
            }
            if (_disposed)
            {
                throw new ObjectDisposedException("BufferStream");
            }

            PrepareToWrite(count);
            System.Buffer.BlockCopy(source, offset, _buffer.Bytes, _position, count);
            AdvancePositionAfterWrite(count);
        }

        public override void WriteByte(byte value)
        {
            PrepareToWrite(1);
            _buffer.Bytes[_position] = value;
            AdvancePositionAfterWrite(1);
        }
    }
}
