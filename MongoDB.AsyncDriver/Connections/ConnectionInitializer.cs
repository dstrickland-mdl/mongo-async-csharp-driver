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
    /// Represents a connection initializer (opens and authenticates connections).
    /// </summary>
    internal static class ConnectionInitializer
    {
        private static async Task<int> DetectConnectionIdAsync(IRootConnection connection, TimeSpan timeout, CancellationToken cancellationToken)
        {
            try
            {
                var command = new BsonDocument("getLastError", 1);
                var operation = new ReadCommandOperation("admin", command);
                var result = await operation.ExecuteAsync(connection, timeout, cancellationToken);
                return result.GetValue("connectionId", -1).ToInt32();
            }
            catch
            {
                // ignore exceptions
                return -1;
            }
        }

        public static async Task InitializeConnectionAsync(IRootConnection connection, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var slidingTimeout = new SlidingTimeout(timeout);

            await connection.OpenAsync(slidingTimeout, cancellationToken);

            var credential = connection.Node.Cluster.Settings.Credential;
            if (credential != null)
            {
                await Authenticator.AuthenticateAsync(connection, credential, slidingTimeout, cancellationToken);
            }

            var connectionId = await DetectConnectionIdAsync(connection, slidingTimeout, cancellationToken);
            connection.SetConnectionId(connectionId);
        }
    }
}
