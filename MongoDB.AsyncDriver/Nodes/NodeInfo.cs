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
    /// Represents information about a node.
    /// </summary>
    public class NodeInfo
    {
        #region static
        public static NodeInfo Create(DnsEndPoint endPoint, NodeState state, BsonDocument isMasterResult, BsonDocument buildInfoResult)
        {
            var isMaster = new IsMasterWrapper(isMasterResult ?? new BsonDocument());
            return new NodeInfo(
                buildInfoResult,
                endPoint,
                isMasterResult,
                isMaster.MaxDocumentSize,
                isMaster.MaxMessageSize,
                isMaster.MaxWireDocumentSize,
                isMaster.GetReplicaSetConfig(endPoint.AddressFamily),
                0,
                state,
                isMaster.Tags,
                isMaster.NodeType);
        }
        #endregion

        // fields
        private readonly BsonDocument _buildInfoResult;
        private readonly DnsEndPoint _endPoint;
        private readonly BsonDocument _isMasterResult;
        private readonly int _maxDocumentSize;
        private readonly int _maxMessageSize;
        private readonly int _maxWireDocumentSize;
        private readonly ReplicaSetConfig _replicaSetConfig;
        private readonly int _revision;
        private readonly NodeState _state;
        private readonly TagSet _tags;
        private readonly NodeType _type;

        // constructors
        public NodeInfo(
            BsonDocument buildInfoResult,
            DnsEndPoint endPoint,
            BsonDocument isMasterResult,
            int maxDocumentSize,
            int maxMessageSize,
            int maxWireDocumentSize,
            ReplicaSetConfig replicaSetConfig,
            int revision,
            NodeState state,
            TagSet tags,
            NodeType type)
        {
            _buildInfoResult = buildInfoResult;
            _endPoint = endPoint;
            _isMasterResult = isMasterResult;
            _maxDocumentSize = maxDocumentSize;
            _maxMessageSize = maxMessageSize;
            _maxWireDocumentSize = maxWireDocumentSize;
            _replicaSetConfig = replicaSetConfig;
            _revision = revision;
            _state = state;
            _tags = tags;
            _type = type;
        }

        // properties
        public BsonDocument BuildInfoResult
        {
            get { return _buildInfoResult; }
        }

        public DnsEndPoint EndPoint
        {
            get { return _endPoint; }
        }

        public BsonDocument IsMasterResult
        {
            get { return _isMasterResult; }
        }

        public int MaxDocumentSize
        {
            get { return _maxDocumentSize; }
        }

        public int MaxMessageSize
        {
            get { return _maxMessageSize; }
        }

        public int MaxWireDocumentSize
        {
            get { return _maxWireDocumentSize; }
        }

        public ReplicaSetConfig ReplicaSetConfig
        {
            get { return _replicaSetConfig; }
        }

        public int Revision
        {
            get { return _revision; }
        }

        public NodeState State
        {
            get { return _state; }
        }

        public TagSet Tags
        {
            get { return _tags; }
        }

        public NodeType Type
        {
            get { return _type; }
        }

        // methods
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(NodeInfo)) { return false; }
            var rhs = (NodeInfo)obj;
            return
                _endPoint.Equals(rhs._endPoint) &&
                _type.Equals(rhs._type) &&
                _state.Equals(rhs._state) &&
                object.Equals(_isMasterResult, rhs._isMasterResult) &&
                object.Equals(_buildInfoResult, rhs._buildInfoResult);
        }

        public override int GetHashCode()
        {
            return new Hasher().Hash(_endPoint).Hash(_type).Hash(_state).Hash(_isMasterResult).Hash(_buildInfoResult).GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{{ EndPoint : {0}, State : {1}, Type : {2}, Revision : {3} }}", DnsEndPointParser.ToString(_endPoint), _state, _type, _revision);
        }

        public NodeInfo WithRevision(int value)
        {
            return (_revision == value) ? this : new NodeInfo(
                _buildInfoResult,
                _endPoint,
                _isMasterResult,
                _maxDocumentSize,
                _maxMessageSize,
                _maxWireDocumentSize,
                _replicaSetConfig,
                value,
                _state,
                _tags,
                _type);
        }
    }
}
