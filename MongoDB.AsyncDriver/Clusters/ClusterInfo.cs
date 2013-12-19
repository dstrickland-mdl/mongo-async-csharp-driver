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
    /// Represents information about a cluster.
    /// </summary>
    public class ClusterInfo
    {
        // fields
        private readonly IReadOnlyList<NodeInfo> _nodes;
        private readonly int _revision;
        private readonly ClusterState _state;
        private readonly ClusterType _type;

        // constructors
        public ClusterInfo(
            ClusterType type,
            ClusterState state,
            IEnumerable<NodeInfo> nodes,
            int revision)
        {
            _type = type;
            _state = state;
            _nodes = (nodes ?? new NodeInfo[0]).ToList();
            _revision = 0;
        }

        public IReadOnlyList<NodeInfo> Nodes
        {
            get { return _nodes; }
        }

        public int Revision
        {
            get { return _revision; }
        }

        public ClusterState State
        {
            get { return _state; }
        }

        public ClusterType Type
        {
            get { return _type; }
        }

        // methods
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj)) { return true; }
            if (obj == null || obj.GetType() != typeof(ClusterInfo)) { return false; }
            var rhs = (ClusterInfo)obj;
            return
                _type.Equals(rhs._type) &&
                _state.Equals(rhs._state) &&
                _nodes.SequenceEqual(rhs._nodes);
        }

        public override int GetHashCode()
        {
            return new Hasher().Hash(_type).Hash(_state).HashEnumerable(_nodes).GetHashCode();
        }

        public override string ToString()
        {
            string nodes;
            if (_nodes.Count == 0)
            {
                nodes = "[ ]";
            }
            else
            {
                nodes = string.Format("[{0}]", string.Join(", ", _nodes.Select(n => n.ToString()).ToArray()));
            }
            return string.Format("{{ Type : {0}, State : {1}, Nodes : {2}, Revision : {3} }}", _type, _state, nodes, _revision);
        }

        public ClusterInfo WithRevision(int value)
        {
            return (_revision == value) ? this : new ClusterInfo(_type, _state, _nodes, Revision);
        }
    }
}
