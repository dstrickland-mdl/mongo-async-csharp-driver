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
    public class NodeInfoChangedEventArgs
    {
        // fields
        private readonly NodeInfo _oldNodeInfo;
        private readonly NodeInfo _newNodeInfo;
        private readonly INode _node;

        // constructors
        public NodeInfoChangedEventArgs(INode node, NodeInfo oldNodeInfo, NodeInfo newNodeInfo)
        {
            _node = node;
            _oldNodeInfo = oldNodeInfo;
            _newNodeInfo = newNodeInfo;
        }

        // properties
        public NodeInfo OldNodeInfo
        {
            get { return _oldNodeInfo; }
        }

        public NodeInfo NewNodeInfo
        {
            get { return _newNodeInfo; }
        }

        public INode Node
        {
            get { return _node; }
        }
    }
}
