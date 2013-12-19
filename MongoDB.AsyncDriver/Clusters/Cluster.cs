﻿/* Copyright 2013-2014 MongoDB Inc.
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
    /// <summary>
    /// Represents a static factory for ICluster implementations.
    /// </summary>
    public static class Cluster
    {
        // static methods
        public static ICluster Create(ClusterSettings settings)
        {
            switch (settings.ClusterType)
            {
                case ClusterType.ReplicaSet:
                    return CreateReplicaSet(settings);
                case ClusterType.StandAlone:
                    return CreateStandaloneCluster(settings);
                default:
                    throw new InvalidOperationException(string.Format("Unsupported cluster type: {0}.", settings.ClusterType));
            }
        }

        public static ICluster Create(string uriString)
        {
            return Create(ClusterSettingsUriParser.Parse(uriString));
        }

        public static ICluster Create(Uri uri)
        {
            return Create(ClusterSettingsUriParser.Parse(uri));
        }

        public static ReplicaSet CreateReplicaSet(ClusterSettings settings)
        {
            var replicaSet = new ReplicaSet(settings);
            foreach (var endPoint in settings.EndPoints)
            {
                var node = new Node(replicaSet, endPoint, settings.NodeSettings);
                replicaSet.AddNode(node);
            }
            return replicaSet;
        }

        public static StandaloneCluster CreateStandaloneCluster(ClusterSettings settings)
        {
            var endPoint = settings.EndPoints.Single();
            var cluster = new StandaloneCluster(settings);
            var node = new Node(cluster, endPoint, settings.NodeSettings);
            cluster.AddNode(node);
            return cluster;
        }
    }
}
