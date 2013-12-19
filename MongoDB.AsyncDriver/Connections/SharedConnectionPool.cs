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
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents a pool of shared connections.
    /// </summary>
    internal class SharedConnectionPool : ConnectionPool
    {
        // fields
        private readonly IConnectionFactory _connectionFactory;
        private bool _disposed;
        private readonly object _lock = new object();
        private readonly IRootNode _node;
        private readonly SharedConnectionPoolSettings _settings;
        private readonly List<SharedConnection> _sharedConnections = new List<SharedConnection>();

        // constructors
        public SharedConnectionPool(IRootNode node, IConnectionFactory connectionFactory, SharedConnectionPoolSettings settings)
        {
            _node = node;
            _connectionFactory = connectionFactory;
            _settings = settings;
        }

        // methods
        public override async Task<IConnection> AcquireConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            SharedConnection connection;

            lock (_lock)
            {
                connection = ChooseAvailableConnection();
                if (connection == null)
                {
                    connection = CreateConnection();
                }
            }

            await connection.LazyInitializeAsync(timeout, cancellationToken);
            return new AcquiredConnection(connection, this);
        }

        private SharedConnection ChooseAvailableConnection()
        {
            var count = _sharedConnections.Count;
            if (count == 0)
            {
                return null;
            }

            var connection = _sharedConnections[0];
            for (var i = 1; connection.NumberOfUsers != 0 && i < count; i++)
            {
                if (connection.NumberOfUsers > _sharedConnections[i].NumberOfUsers)
                {
                    connection = _sharedConnections[i];
                }
            }

            if (connection.NumberOfUsers != 0 && _sharedConnections.Count < _settings.MaxConnections)
            {
                return null;
            }

            connection.IncrementNumberOfUsers();
            return connection;
        }

        private SharedConnection CreateConnection()
        {
            var wrapped = _connectionFactory.CreateConnection(_node); // will be initialized by caller (outside of the lock)
            var connection = new SharedConnection(wrapped);
            _sharedConnections.Add(connection);
            return connection;
        }

        protected override void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    _disposed = true;

                    foreach (var connection in _sharedConnections)
                    {
                        connection.Dispose();
                    }
                }
            }
        }

        public override void ReleaseConnection(IConnection connection)
        {
            var sharedConnection = (SharedConnection)connection;
            sharedConnection.DecrementNumberOfUsers();
        }
    }
}
