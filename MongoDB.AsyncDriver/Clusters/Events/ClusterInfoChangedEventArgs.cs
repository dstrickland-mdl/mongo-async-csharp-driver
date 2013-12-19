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
    public class ClusterInfoChangedEventArgs
    {
        // fields
        private readonly ClusterInfo _oldClusterInfo;
        private readonly ClusterInfo _newClusterInfo;
        private readonly ICluster _cluster;

        // constructors
        public ClusterInfoChangedEventArgs(ICluster cluster, ClusterInfo oldClusterInfo, ClusterInfo newClusterInfo)
        {
            _cluster = cluster;
            _oldClusterInfo = oldClusterInfo;
            _newClusterInfo = newClusterInfo;
        }

        // properties
        public ICluster Cluster
        {
            get { return _cluster; }
        }

        public ClusterInfo OldClusterInfo
        {
            get { return _oldClusterInfo; }
        }

        public ClusterInfo NewClusterInfo
        {
            get { return _newClusterInfo; }
        }
    }
}
