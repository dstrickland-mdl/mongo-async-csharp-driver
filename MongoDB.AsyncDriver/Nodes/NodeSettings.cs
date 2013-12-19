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
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents settings for a node.
    /// </summary>
    public class NodeSettings
    {
        // fields
        private readonly IConnectionPoolFactory _connectionPoolFactory;
        private readonly TimeSpan _pingInterval;
        private readonly TimeSpan _pingTimeout;

        // constructors
        public NodeSettings()
        {
            _connectionPoolFactory = new ConnectionPoolFactory();
            _pingInterval = TimeSpan.FromSeconds(10);
            _pingTimeout = TimeSpan.FromSeconds(10);
        }

        internal NodeSettings(
            IConnectionPoolFactory connectionPoolFactory,
            TimeSpan pingInterval,
            TimeSpan pingTimeout)
        {
            _connectionPoolFactory = connectionPoolFactory;
            _pingInterval = pingInterval;
            _pingTimeout = pingTimeout;
        }

        // properties
        public IConnectionPoolFactory ConnectionPoolFactory
        {
            get { return _connectionPoolFactory; }
        }

        public TimeSpan PingInterval
        {
            get { return _pingInterval; }
        }

        public TimeSpan PingTimeout
        {
            get { return _pingTimeout; }
        }

        // methods
        public NodeSettings WithConnectionPoolFactory(IConnectionPoolFactory value)
        {
            return object.ReferenceEquals(_connectionPoolFactory, value) ? this : new NodeSettings(value, _pingInterval, _pingTimeout);
        }

        public NodeSettings WithPingInterval(TimeSpan value)
        {
            return (_pingInterval == value) ? this : new NodeSettings(_connectionPoolFactory, value, _pingTimeout);
        }

        public NodeSettings WithPingTimeout(TimeSpan value)
        {
            return (_pingTimeout == value) ? this : new NodeSettings(_connectionPoolFactory, _pingInterval, value);
        }
    }
}
