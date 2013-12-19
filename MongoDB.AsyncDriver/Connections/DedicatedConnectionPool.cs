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
    /// Represents a pool of dedicated connections.
    /// </summary>
    internal class DedicatedConnectionPool : ConnectionPool, IDedicatedConnectionPool
    {
        // fields
        private readonly List<DedicatedConnection> _acquiredConnections = new List<DedicatedConnection>();
        private readonly List<DedicatedConnection> _availableConnections = new List<DedicatedConnection>();
        private readonly Queue<TaskCompletionSource<DedicatedConnection>> _awaiters = new Queue<TaskCompletionSource<DedicatedConnection>>();
        private readonly IConnectionFactory _connectionFactory;
        private bool _disposed;
        private readonly object _lock = new object();
        private readonly IRootNode _node;
        private readonly DedicatedConnectionPoolSettings _settings;

        // constructors
        public DedicatedConnectionPool(IRootNode node, IConnectionFactory connectionFactory, DedicatedConnectionPoolSettings settings)
        {
            _node = node;
            _connectionFactory = connectionFactory;
            _settings = settings;
        }

        // methods
        public override async Task<IConnection> AcquireConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            DedicatedConnection availableConnection = null;
            DedicatedConnection newConnection = null;
            Task<DedicatedConnection> waitQueueTask = null;

            lock (_lock)
            {
                availableConnection = ChooseAvailableConnection();
                if (availableConnection == null)
                {
                    if (!PoolIsFull())
                    {
                        newConnection = CreateConnection();
                    }
                    else if (!WaitQueueIsFull())
                    {
                        waitQueueTask = CreateWaitQueueTask();
                    }
                }
            }

            if (availableConnection != null)
            {
                return new AcquiredConnection(availableConnection, this);
            }
            else if (newConnection != null)
            {
                await InitializeNewConnectionAsync(newConnection, timeout, cancellationToken);
                return new AcquiredConnection(newConnection, this);
            }
            else if (waitQueueTask != null)
            {
                var returnedConnection = await waitQueueTask;
                return new AcquiredConnection(returnedConnection, this);
            }
            else
            {
                throw new InvalidOperationException("The maximum number of dedicated connections are already in use and the wait queue is full.");
            }
        }

        private DedicatedConnection ChooseAvailableConnection()
        {
            var count = _availableConnections.Count;
            if (count == 0)
            {
                return null;
            }

            var last = count - 1;
            var connection = _availableConnections[last];
            _availableConnections.RemoveAt(last);
            _acquiredConnections.Add(connection);
            return connection;
        }

        private DedicatedConnection CreateConnection()
        {
            var wrapped = _connectionFactory.CreateConnection(_node); // will be initialized by caller (outside of the lock)
            var connection = new DedicatedConnection(wrapped);
            _acquiredConnections.Add(connection);
            return connection;
        }

        public async Task<IConnection> CreateUnpooledConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            var connection = _connectionFactory.CreateConnection(_node);
            try
            {
                await ConnectionInitializer.InitializeConnectionAsync(connection, timeout, cancellationToken);
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        private Task<DedicatedConnection> CreateWaitQueueTask()
        {
            var awaiter = new TaskCompletionSource<DedicatedConnection>();
            _awaiters.Enqueue(awaiter);
            return awaiter.Task;
        }

        protected override void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    if (disposing)
                    {
                        var connectionPoolDisposedException = new ObjectDisposedException("ConnectionPool");
                        foreach (var awaiter in _awaiters)
                        {
                            awaiter.TrySetException(connectionPoolDisposedException);
                        }

                        foreach (var connection in _acquiredConnections)
                        {
                            connection.Dispose();
                        }

                        foreach (var connection in _availableConnections)
                        {
                            connection.Dispose();
                        }
                    }
                }
            }
        }

        private async Task InitializeNewConnectionAsync(DedicatedConnection connection, TimeSpan timeout, CancellationToken cancellationToken)
        {
            try
            {
                await ConnectionInitializer.InitializeConnectionAsync(connection.Wrapped, timeout, cancellationToken);
            }
            catch
            {
                lock (_lock)
                {
                    _acquiredConnections.Remove(connection);
                }
                throw;
            }
        }

        private bool PoolIsFull()
        {
            var total = _availableConnections.Count + _acquiredConnections.Count;
            return total >= _settings.MaxConnections;
        }

        public override void ReleaseConnection(IConnection connection)
        {
            var dedicatedConnection = (DedicatedConnection)connection;
            var referenceCount = dedicatedConnection.DecrementReferenceCount();
            if (referenceCount > 0)
            {
                return;
            }

            TaskCompletionSource<DedicatedConnection> awaiter;

            lock (_lock)
            {
                if (_awaiters.Count > 0)
                {
                    awaiter = _awaiters.Dequeue();
                }
                else
                {
                    _acquiredConnections.Remove(dedicatedConnection);
                    _availableConnections.Add(dedicatedConnection);
                    return;
                }
            }

            awaiter.TrySetResult(dedicatedConnection);
        }

        private bool WaitQueueIsFull()
        {
            return _awaiters.Count >= _settings.MaxWaitQueueSize;
        }
    }
}
