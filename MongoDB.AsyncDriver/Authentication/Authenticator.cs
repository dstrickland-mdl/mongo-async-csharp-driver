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
    public static class Authenticator
    {
        // static fields
        private static readonly IAuthenticationProtocol[] _protocols;

        // static constructor
        static Authenticator()
        {
            _protocols = new IAuthenticationProtocol[]
            {
                new OriginalAuthenticationProtocol(),
                new X509AuthenticationProtocol()
            };
        }

        // methods
        public static async Task AuthenticateAsync(IRootConnection connection, ICredential credential, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var slidingTimeout = new SlidingTimeout(timeout);
            if (connection.Node.Cluster.Info.Type == ClusterType.ReplicaSet && await IsArbiterAsync(connection, slidingTimeout, cancellationToken))
            {
                // don't authenticate against arbiters (at least not until server is fixed to support authentication against arbiters)
                return;
            }

            foreach (var protocol in _protocols)
            {
                if (protocol.CanUse(credential))
                {
                    await protocol.AuthenticateAsync(connection, credential, slidingTimeout, cancellationToken);
                    return;
                }
            }
        }

        private static async Task<bool> IsArbiterAsync(IRootConnection connection, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var command = new BsonDocument("isMaster", true);
            var operation = new ReadCommandOperation("admin", command);
            var result = await operation.ExecuteAsync(connection, timeout, cancellationToken);
            return result.GetValue("arbiterOnly", false).ToBoolean();
        }
    }
}
