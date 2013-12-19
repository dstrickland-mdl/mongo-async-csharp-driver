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
    /// Represents information about a replica set cluster.
    /// </summary>
    public class ReplicaSetInfo : ClusterInfo
    {
        // fields
        private readonly ReplicaSetConfig _config;

        // constructors
        public ReplicaSetInfo(
            ClusterType type,
            ClusterState state,
            IEnumerable<NodeInfo> nodes,
            ReplicaSetConfig config,
            int revision)
            : base(type, state, nodes, revision)
        {
            _config = config ?? new ReplicaSetConfig(new DnsEndPoint[0], null, null, null);
        }

        // properties
        public ReplicaSetConfig Config
        {
            get { return _config; }
        }

        // methods
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj)) { return true; }
            if (obj == null || obj.GetType() != typeof(ReplicaSetInfo)) { return false; }
            var rhs = (ReplicaSetInfo)obj;
            return
                base.Equals(obj) &&
                object.Equals(_config, rhs._config);
        }

        public override int GetHashCode()
        {
            return new Hasher()
                .Hash(base.GetHashCode())
                .Hash(_config)
                .GetHashCode();
        }

        public ReplicaSetInfo WithNodeInfo(NodeInfo node)
        {
            var currentNodeInfo = Nodes.First(n => n.EndPoint == node.EndPoint);
            if (currentNodeInfo.Equals(node))
            {
                return this;
            }

            var nodes = new List<NodeInfo>(Nodes.Where(n => n.EndPoint != node.EndPoint));
            var config = _config;

            if (node.ReplicaSetConfig != null)
            {
                if (config == null || node.ReplicaSetConfig.Name == config.Name)
                {
                    config = node.ReplicaSetConfig;
                    nodes.Add(node);
                }

                foreach (var member in config.Members)
                {
                    if (!nodes.Any(n => n.EndPoint == member))
                    {
                        var addedMember = NodeInfo.Create(member, NodeState.Disconnected, null, null);
                        nodes.Add(addedMember);
                    }
                }

                foreach (var existingNode in nodes.ToList())
                {
                    if (!config.Members.Contains(existingNode.EndPoint))
                    {
                        nodes.Remove(existingNode);
                    }
                }
            }

            var state =
                nodes.All(n => n.State == NodeState.Connected) ? ClusterState.Connected :
                nodes.Any(n => n.State == NodeState.Connected) ? ClusterState.PartiallyConnected :
                ClusterState.Disconnected;

            return new ReplicaSetInfo(ClusterType.ReplicaSet, state, nodes.OrderBy(n => n.EndPoint), config, 0);
        }

        public new ReplicaSetInfo WithRevision(int value)
        {
            return (Revision == value) ? this : new ReplicaSetInfo(Type, State, Nodes, _config, value);
        }
    }
}
