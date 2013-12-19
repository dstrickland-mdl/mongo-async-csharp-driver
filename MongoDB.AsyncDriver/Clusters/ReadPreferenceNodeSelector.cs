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
    /// Represents a selector that chooses a node from a cluster based on a read preference.
    /// </summary>
    internal static class ReadPreferenceNodeSelector
    {
        // static fields
        private static readonly Random __random = new Random();
        private static readonly object __randomLock = new object();

        // static methods
        public static DnsEndPoint ChooseNode(ReplicaSetInfo info, ReadPreference readPreference)
        {
            switch (readPreference.Mode)
            {
                case ReadPreferenceMode.Primary:
                    return info.Config.Primary;

                case ReadPreferenceMode.PrimaryPreferred:
                    if (info.Config.Primary != null)
                    {
                        return info.Config.Primary;
                    }
                    else
                    {
                        return ChooseNode(GetSecondaries(info), readPreference.TagSets);
                    }

                case ReadPreferenceMode.Secondary:
                    return ChooseNode(GetSecondaries(info), readPreference.TagSets);

                case ReadPreferenceMode.SecondaryPreferred:
                    var secondary = ChooseNode(GetSecondaries(info), readPreference.TagSets);
                    if (secondary != null)
                    {
                        return secondary;
                    }
                    else
                    {
                        return info.Config.Primary;
                    }

                case ReadPreferenceMode.Nearest:
                    return ChooseNode(GetPrimaryAndSecondaries(info), readPreference.TagSets);

                default:
                    throw new ArgumentException("Invalid ReadPreferenceMode.");
            }
        }

        private static DnsEndPoint ChooseNode(IEnumerable<NodeInfo> nodes, IReadOnlyList<TagSet> tagSets)
        {
            var nodesList = new List<NodeInfo>(nodes);

            foreach (var tagSet in (tagSets ?? new[] { new TagSet() }))
            {
                var matchingNodes = new List<NodeInfo>();
                foreach (var node in nodesList)
                {
                    if (node.Tags.ContainsAll(tagSet))
                    {
                        matchingNodes.Add(node);
                    }
                }

                // TODO: take maxAcceptableLatency into account

                if (matchingNodes.Count == 1)
                {
                    return matchingNodes[0].EndPoint;
                }
                else if (matchingNodes.Count != 0)
                {
                    lock (__randomLock)
                    {
                        var index = __random.Next(matchingNodes.Count);
                        return matchingNodes[index].EndPoint;
                    }
                }
            }

            return null;
        }

        private static IEnumerable<NodeInfo> GetPrimaryAndSecondaries(ReplicaSetInfo info)
        {
            return info.Nodes.Where(n => n.Type == NodeType.Primary || n.Type == NodeType.Secondary);
        }

        private static IEnumerable<NodeInfo> GetSecondaries(ReplicaSetInfo info)
        {
            return info.Nodes.Where(n => n.Type == NodeType.Secondary);
        }
    }
}
