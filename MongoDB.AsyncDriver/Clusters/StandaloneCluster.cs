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
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents a direct connection to a single standalone node.
    /// </summary>
    public class StandaloneCluster : ICluster
    {
        // fields
        private bool _disposed;
        private ClusterInfo _info;
        private TaskCompletionSource<bool> _infoChangedTaskCompletionSource;
        private readonly object _lock = new object();
        private Node _node;
        private readonly ClusterSettings _settings;

        // events
        public event EventHandler InfoChanged;

        // constructor
        internal StandaloneCluster(ClusterSettings settings)
        {
            _settings = settings;
            _info = new ClusterInfo(ClusterType.StandAlone, ClusterState.Disconnected, new NodeInfo[0], 0);
            _infoChangedTaskCompletionSource = new TaskCompletionSource<bool>();
        }

        // properties
        public ClusterInfo Info
        {
            get
            {
                ThrowIfDisposed();
                lock (_lock)
                {
                    return _info;
                }
            }
        }

        public IReadOnlyList<INode> Nodes
        {
            get
            {
                ThrowIfDisposed();
                return new[] { new WritableNode(_node) };
            }
        }

        public IReadOnlyList<IReadableNode> ReadableNodes
        {
            get
            {
                ThrowIfDisposed();
                return new[] { new ReadableNode(_node) };
            }
        }

        public ClusterSettings Settings
        {
            get
            {
                ThrowIfDisposed();
                return _settings;
            }
        }

        public IReadOnlyList<IWritableNode> WritableNodes
        {
            get
            {
                ThrowIfDisposed();
                return new[] { new WritableNode(_node) };
            }
        }

        // methods
        internal void AddNode(Node node)
        {
            _node = node;
            _node.InfoChanged += NodeInfoChangedHandler;
            _node.StartBackgroundTasks();

            var clusterListener = _settings.ClusterListener;
            if (clusterListener != null)
            {
                var args = new NodeAddedEventArgs(node);
                clusterListener.NodeAdded(args);
            }
        }

        private void CheckIfInfoChanged(ClusterInfo newInfo)
        {
            var infoChanged = false;
            ClusterInfo oldInfo = null;
            TaskCompletionSource<bool> oldInfoChangedTaskCompletionSource = null;

            lock (_lock)
            {
                if (!_info.Equals(newInfo))
                {
                    infoChanged = true;
                    oldInfo = _info;
                    oldInfoChangedTaskCompletionSource = _infoChangedTaskCompletionSource;
                    _info = newInfo.WithRevision(oldInfo.Revision + 1);
                    _infoChangedTaskCompletionSource = new TaskCompletionSource<bool>();
                }
            }

            if (infoChanged)
            {
                oldInfoChangedTaskCompletionSource.TrySetResult(true);
                OnInfoChanged(oldInfo, newInfo);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _node.Dispose();
                }
            }
        }

        public async Task<ClusterInfo> GetInfoAsync(int minimumRevision = 0, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            var slidingTimeout = new SlidingTimeout(timeout);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ClusterInfo info;
                Task infoChangedTask;
                lock (_lock)
                {
                    info = _info;
                    infoChangedTask = _infoChangedTaskCompletionSource.Task;
                }

                if (info.Revision >= minimumRevision)
                {
                    return info;
                }

                await infoChangedTask.WithTimeout(timeout, cancellationToken);
            }
        }

        public async Task<IReadableNode> GetReadableNodeAsync(ReadPreference readPreference, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            var slidingTimeout = new SlidingTimeout(timeout);

            var nodeInfo = _node.Info;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (nodeInfo.Type == NodeType.StandAlone)
                {
                    return new ReadableNode(_node);
                }

                if (nodeInfo.Type != NodeType.Unknown)
                {
                    throw new Exception("Not connected to a stand alone server.");
                }

                nodeInfo = await _node.GetInfoAsync(nodeInfo.Revision + 1, slidingTimeout, cancellationToken);
            }
        }

        public async Task<IWritableNode> GetWritableNodeAsync(TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            var slidingTimeout = new SlidingTimeout(timeout);

            var nodeInfo = _node.Info;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (nodeInfo.Type == NodeType.StandAlone)
                {
                    return new WritableNode(_node);
                }

                if (nodeInfo.Type != NodeType.Unknown)
                {
                    throw new Exception("Not connected to a stand alone server.");
                }

                nodeInfo = await _node.GetInfoAsync(nodeInfo.Revision + 1, slidingTimeout, cancellationToken);
            }
        }

        private void NodeInfoChangedHandler(object sender, EventArgs args)
        {
            var node = (INode)sender;
            var newNodeInfo = node.Info;
            var newClusterState = (newNodeInfo.State == NodeState.Connected) ? ClusterState.Connected : ClusterState.Disconnected;
            var newClusterInfo = new ClusterInfo(ClusterType.StandAlone, newClusterState, new[] { newNodeInfo }, 0);
            CheckIfInfoChanged(newClusterInfo);
        }

        private void OnInfoChanged(ClusterInfo oldInfo, ClusterInfo newInfo)
        {
            var clusterListener = _settings.ClusterListener;
            if (clusterListener != null)
            {
                var args = new ClusterInfoChangedEventArgs(this, oldInfo, newInfo);
                clusterListener.ClusterInfoChanged(args);
            }

            var handler = InfoChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("StandAloneCluster");
            }
        }
    }
}
