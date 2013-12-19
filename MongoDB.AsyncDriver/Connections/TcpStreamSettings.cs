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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents settings for a TCP stream.
    /// </summary>
    public class TcpStreamSettings
    {
        // fields
        private readonly TimeSpan _connectTimeout;
        private readonly TimeSpan _readTimeout;
        private readonly int _receiveBufferSize;
        private readonly int _sendBufferSize;
        private readonly TimeSpan _writeTimeout;

        // constructors
        public TcpStreamSettings()
        {
            _connectTimeout = TimeSpan.FromSeconds(10);
            _readTimeout = TimeSpan.Zero;
            _receiveBufferSize = 64 * 1024;
            _sendBufferSize = 64 * 1024;
            _writeTimeout = TimeSpan.Zero;
        }

        private TcpStreamSettings(
            TimeSpan connectTimeout,
            TimeSpan readTimeout,
            int receiveBufferSize,
            int sendBufferSize,
            TimeSpan writeTimeout)
        {
            _connectTimeout = connectTimeout;
            _readTimeout = readTimeout;
            _receiveBufferSize = receiveBufferSize;
            _sendBufferSize = sendBufferSize;
            _writeTimeout = writeTimeout;
        }

        // properties
        public TimeSpan ConnectTimeout
        {
            get { return _connectTimeout; }
        }

        public TimeSpan ReadTimeout
        {
            get { return _readTimeout; }
        }

        public int ReceiveBufferSize
        {
            get { return _receiveBufferSize; }
        }

        public int SendBufferSize
        {
            get { return _sendBufferSize; }
        }

        public TimeSpan WriteTimeout
        {
            get { return _writeTimeout; }
        }

        // methods
        public TcpStreamSettings WithConnectTimeout(TimeSpan value)
        {
            return (_connectTimeout == value) ? this : new Builder(this) { _connectTimeout = value }.Build();
        }

        public TcpStreamSettings WithReadTimeout(TimeSpan value)
        {
            return (_readTimeout == value) ? this : new Builder(this) { _readTimeout = value }.Build();
        }

        public TcpStreamSettings WithReceiveBufferSize(int value)
        {
            return (_receiveBufferSize == value) ? this : new Builder(this) { _receiveBufferSize = value }.Build();
        }

        public TcpStreamSettings WithSendBufferSize(int value)
        {
            return (_sendBufferSize == value) ? this : new Builder(this) { _sendBufferSize = value }.Build();
        }

        public TcpStreamSettings WithWriteTimeout(TimeSpan value)
        {
            return (_writeTimeout == value) ? this : new Builder(this) { _writeTimeout = value }.Build();
        }

        // nested types
        private struct Builder
        {
            // fields
            public TimeSpan _connectTimeout;
            public TimeSpan _readTimeout;
            public int _receiveBufferSize;
            public int _sendBufferSize;
            public TimeSpan _writeTimeout;

            // constructors
            public Builder(TcpStreamSettings other)
            {
                _connectTimeout = other.ConnectTimeout;
                _readTimeout = other.ReadTimeout;
                _receiveBufferSize = other.ReceiveBufferSize;
                _sendBufferSize = other.SendBufferSize;
                _writeTimeout = other.WriteTimeout;
            }

            // methods
            public TcpStreamSettings Build()
            {
                return new TcpStreamSettings(
                    _connectTimeout,
                    _readTimeout,
                    _receiveBufferSize,
                    _sendBufferSize,
                    _writeTimeout);
            }
        }
    }
}
