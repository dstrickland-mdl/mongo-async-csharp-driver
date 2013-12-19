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
    /// Represents settings for a cluster.
    /// </summary>
    public class ClusterSettings
    {
        // fields
        private readonly IClusterListener _clusterListener;
        private readonly ClusterType _clusterType;
        private readonly ICredential _credential;
        private readonly IReadOnlyList<DnsEndPoint> _endPoints;
        private readonly IMessageListener _messageListener;
        private readonly NodeSettings _nodeSettings;

        // constructors
        public ClusterSettings()
        {
            _clusterType = ClusterType.StandAlone;
            _endPoints = new DnsEndPoint[0];
            _nodeSettings = new NodeSettings();
        }

        internal ClusterSettings(
            IClusterListener clusterListener,
            ClusterType clusterType,
            ICredential credential,
            IReadOnlyList<DnsEndPoint> endPoints,
            IMessageListener messageListener,
            NodeSettings nodeSettings)
        {
            _clusterListener = clusterListener;
            _clusterType = clusterType;
            _credential = credential;
            _endPoints = endPoints;
            _messageListener = messageListener;
            _nodeSettings = nodeSettings;
        }

        public ClusterSettings(string uriString)
            : this(new Uri(uriString))
        {
        }

        public ClusterSettings(Uri uri)
        {
            var parsed = ClusterSettingsUriParser.Parse(uri);
            _clusterListener = parsed._clusterListener;
            _clusterType = parsed._clusterType;
            _credential = parsed._credential;
            _endPoints = parsed._endPoints;
            _messageListener = parsed._messageListener;
            _nodeSettings = parsed._nodeSettings;
        }

        // properties
        public IClusterListener ClusterListener
        {
            get { return _clusterListener; }
        }

        public ClusterType ClusterType
        {
            get { return _clusterType; }
        }

        public ICredential Credential
        {
            get { return _credential; }
        }

        public IReadOnlyList<DnsEndPoint> EndPoints
        {
            get { return _endPoints; }
        }

        public IMessageListener MessageListener
        {
            get { return _messageListener; }
        }

        public NodeSettings NodeSettings
        {
            get { return _nodeSettings; }
        }

        // methods
        public ClusterSettings WithClusterListener(IClusterListener value)
        {
            return object.ReferenceEquals(_clusterListener, value) ? this : new Builder(this) { _clusterListener = value }.Build();
        }

        public ClusterSettings WithClusterType(ClusterType value)
        {
            return (_clusterType == value) ? this : new Builder(this) { _clusterType = value }.Build();
        }

        public ClusterSettings WithCredential(ICredential value)
        {
            return object.Equals(_credential, value) ? this : new Builder(this) { _credential = value }.Build();
        }

        public ClusterSettings WithEndPoints(IReadOnlyList<DnsEndPoint> value)
        {
            return object.ReferenceEquals(_endPoints, value) ? this : new Builder(this) { _endPoints = value }.Build();
        }

        public ClusterSettings WithMessageListener(IMessageListener value)
        {
            return object.ReferenceEquals(_messageListener, value) ? this : new Builder(this) { _messageListener = value }.Build();
        }

        public ClusterSettings WithNodeSettings(NodeSettings value)
        {
            return object.Equals(_nodeSettings, value) ? this : new Builder(this) { _nodeSettings = value }.Build();
        }

        // nested types
        private struct Builder
        {
            // fields
            public IClusterListener _clusterListener;
            public ClusterType _clusterType;
            public ICredential _credential;
            public IReadOnlyList<DnsEndPoint> _endPoints;
            public IMessageListener _messageListener;
            public NodeSettings _nodeSettings;

            // constructors
            public Builder(ClusterSettings other)
            {
                _clusterListener = other.ClusterListener;
                _clusterType = other.ClusterType;
                _credential = other.Credential;
                _endPoints = other.EndPoints;
                _messageListener = other.MessageListener;
                _nodeSettings = other.NodeSettings;
            }

            // methods
            public ClusterSettings Build()
            {
                return new ClusterSettings(
                    _clusterListener,
                    _clusterType,
                    _credential,
                    _endPoints,
                    _messageListener,
                    _nodeSettings);
            }
        }
    }
}
