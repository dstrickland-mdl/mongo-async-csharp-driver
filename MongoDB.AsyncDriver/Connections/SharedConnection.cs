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
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents a shared connection.
    /// </summary>
    internal class SharedConnection : PooledConnection
    {
        // fields
        private Task<IConnection> _initializeTask;
        private object _lock = new object();
        private int _numberOfUsers;

        // constructors
        public SharedConnection(IRootConnection wrapped)
            : base(wrapped)
        {
            _numberOfUsers = 1;
        }

        // properties
        public int NumberOfUsers
        {
            get
            {
                ThrowIfDisposed();
                return _numberOfUsers;
            }
            set
            {
                ThrowIfDisposed();
                _numberOfUsers = value;
            }
        }

        // methods
        public void DecrementNumberOfUsers()
        {
            Interlocked.Decrement(ref _numberOfUsers);
        }

        public override IConnection Fork()
        {
            IncrementNumberOfUsers();
            return this;
        }

        public void IncrementNumberOfUsers()
        {
            Interlocked.Increment(ref _numberOfUsers);
        }

        public Task<IConnection> LazyInitializeAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            lock (_lock)
            {
                if (_initializeTask == null)
                {
                    _initializeTask = LazyInitializeAsyncHelper(timeout, cancellationToken);
                }
            }
            return _initializeTask;
        }

        private async Task<IConnection> LazyInitializeAsyncHelper(TimeSpan timeout, CancellationToken cancellationToken)
        {
            await ConnectionInitializer.InitializeConnectionAsync(Wrapped, timeout, cancellationToken);
            return this;
        }
    }
}
