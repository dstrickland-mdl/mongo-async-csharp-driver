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
    public class MockConnectionPool : IDedicatedConnectionPool
    {
        // fields
        private readonly IConnectionFactory _connectionFactory;
        private readonly IRootNode _node;

        // constructors
        public MockConnectionPool(IRootNode node, IConnectionFactory connectionFactory)
        {
            _node = node;
            _connectionFactory = connectionFactory;
        }

        // methods
        public Task<IConnection> AcquireConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return CreateUnpooledConnectionAsync(timeout, cancellationToken);
        }

        public void Dispose()
        {
        }

        public async Task<IConnection> CreateUnpooledConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            var connection = _connectionFactory.CreateConnection(_node);
            await ConnectionInitializer.InitializeConnectionAsync(connection, timeout, cancellationToken);
            return connection;
        }

        public void ReleaseConnection(IConnection connection)
        {
        }
    }
}
