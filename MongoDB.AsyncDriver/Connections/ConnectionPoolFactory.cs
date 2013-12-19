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
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents a connection pool factory.
    /// </summary>
    public class ConnectionPoolFactory : IConnectionPoolFactory
    {
        // fields
        private readonly IConnectionFactory _connectionFactory;
        private readonly DedicatedConnectionPoolSettings _dedicatedConnectionPoolSettings;
        private readonly SharedConnectionPoolSettings _sharedConnectionPoolSettings;

        // constructors
        public ConnectionPoolFactory()
        {
            _connectionFactory = new BinaryConnectionFactory();
            _dedicatedConnectionPoolSettings = new DedicatedConnectionPoolSettings();
            _sharedConnectionPoolSettings = new SharedConnectionPoolSettings();
        }

        public ConnectionPoolFactory(IConnectionFactory connectionFactory, DedicatedConnectionPoolSettings dedicatedConnectionPoolSettings, SharedConnectionPoolSettings sharedConnectionPoolSettings)
        {
            _connectionFactory = connectionFactory;
            _dedicatedConnectionPoolSettings = dedicatedConnectionPoolSettings;
            _sharedConnectionPoolSettings = sharedConnectionPoolSettings;
        }

        // methods
        public IDedicatedConnectionPool CreateDedicatedConnectionPool(IRootNode node)
        {
            return new DedicatedConnectionPool(node, _connectionFactory, _dedicatedConnectionPoolSettings);
        }

        public IConnectionPool CreateSharedConnectionPool(IRootNode node)
        {
            return new SharedConnectionPool(node, _connectionFactory, _sharedConnectionPoolSettings);
        }
    }
}
