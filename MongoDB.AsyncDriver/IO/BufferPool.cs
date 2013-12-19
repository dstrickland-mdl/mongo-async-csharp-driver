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
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public class BufferPool : IBufferPool
    {
        #region static
        // static fields
        private static IBufferPool __default = new BufferPool(512 * (1024 * 1024), 64 * (1024 * 1024));

        // static properties
        public static IBufferPool Default
        {
            get { return __default; }
            set { __default = value; }
        }
        #endregion

        // fields
        private readonly BufferManager _bufferManager;
        private bool _disposed;

        // constructors
        public BufferPool(long maxPoolSize, int maxBufferSize)
        {
            _bufferManager = BufferManager.CreateBufferManager(maxPoolSize, maxBufferSize);
        }

        // methods
        public IBuffer Acquire(int size)
        {
            if (_disposed) { throw new ObjectDisposedException("BufferPool"); }
            var bytes = _bufferManager.TakeBuffer(size);
            return new PooledBuffer(bytes, size, _bufferManager);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _bufferManager.Clear();
            }
        }

        // nested classes
        private class PooledBuffer : IBuffer
        {
            // fields
            private readonly BufferManager _bufferManager;
            private byte[] _bytes;
            private bool _disposed;
            private int _length;

            // constructors
            public PooledBuffer(byte[] bytes, int length, BufferManager bufferManager)
            {
                _bytes = bytes;
                _length = length;
                _bufferManager = bufferManager;
            }

            // properties
            public byte[] Bytes
            {
                get
                {
                    if (_disposed) { throw new ObjectDisposedException("PooledBuffer"); }
                    return _bytes;
                }
            }

            public int Length
            {
                get
                {
                    if (_disposed)
                    {
                        throw new ObjectDisposedException("PooledBuffer");
                    }
                    return _length;
                }
                set
                {
                    if (value < 0)
                    {
                        throw new ArgumentException("Length is negative.", "value");
                    }
                    if (_disposed)
                    {
                        throw new ObjectDisposedException("PooledBuffer");
                    }
                    if (value > _bytes.Length)
                    {
                        throw new ArgumentException("Length is greater than the length of the byte array.", "value");
                    }
                    _length = value;
                }
            }

            // methods
            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _bufferManager.ReturnBuffer(_bytes);
                }
            }
        }
    }
}
