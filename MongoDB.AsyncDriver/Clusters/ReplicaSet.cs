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
    /// Represents a replica set cluster.
    /// </summary>
    public class ReplicaSet : IReplicaSet
    {
        // fields
        private readonly CancellationTokenSource _backgroundTaskCancellationTokenSource = new CancellationTokenSource();
        private bool _disposed;
        private readonly object _lock = new object();
        private ReplicaSetInfo _info;
        private TaskCompletionSource<bool> _infoChangedTaskCompletionSource;
        private readonly AsyncQueue<NodeInfo> _nodeInfoChangedQueue = new AsyncQueue<NodeInfo>();
        private readonly List<Node> _nodes = new List<Node>();
        private IWritableNode _primary;
        private readonly ClusterSettings _settings;

        // constructors
        public ReplicaSet(ClusterSettings settings)
        {
            _settings = settings;
            var nodes = settings.EndPoints.Select(e => NodeInfo.Create(e, NodeState.Disconnected, null, null));
            _info = new ReplicaSetInfo(ClusterType.ReplicaSet, ClusterState.Disconnected, nodes, null, 0);
            _infoChangedTaskCompletionSource = new TaskCompletionSource<bool>();
        }

        // events
        public event EventHandler InfoChanged;

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
                lock (_lock)
                {
                    return _nodes
                        .Select(n => n.Info.Type == NodeType.Primary ? new WritableNode(n) : new ReadableNode(n))
                        .ToList();
                }
            }
        }

        public IWritableNode Primary
        {
            get
            {
                ThrowIfDisposed();
                lock (_lock)
                {
                    return _primary;
                }
            }
        }

        public IReadOnlyList<IReadableNode> ReadableNodes
        {
            get
            {
                ThrowIfDisposed();
                lock (_lock)
                {
                    return _nodes
                        .Where(n => n.Info.Type == NodeType.Primary || n.Info.Type == NodeType.Secondary)
                        .Select(n => new ReadableNode(n))
                        .ToList();
                }
            }
        }

        public IReadOnlyList<IReadableNode> Secondaries
        {
            get
            {
                ThrowIfDisposed();
                lock (_lock)
                {
                    return _nodes
                        .Where(n => n.Info.Type == NodeType.Secondary)
                        .Select(n => new ReadableNode(n))
                        .ToList();
                }
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
                lock (_lock)
                {
                    return (_primary == null) ? new IWritableNode[0] : new[] { _primary };
                }
            }
        }

        // methods
        internal void AddNode(Node node)
        {
            lock (_lock)
            {
                if (_nodes.Any(n => n.EndPoint == node.EndPoint))
                {
                    var message = string.Format("The replica set already contains a node for end point: {0}.", DnsEndPointParser.ToString(node.EndPoint));
                    throw new ArgumentException(message, "node");
                }

                _nodes.Add(node);
            }

            node.InfoChanged += NodeInfoChangedHandler;
            node.StartBackgroundTasks();

            var clusterListener = _settings.ClusterListener;
            if (clusterListener != null)
            {
                var args = new NodeAddedEventArgs(node);
                clusterListener.NodeAdded(args);
            }
        }

        private void AddOrRemoveNodes(ReplicaSetConfig config)
        {
            foreach (var endPoint in config.Members)
            {
                if (!_nodes.Any(n => n.EndPoint == endPoint))
                {
                    var node = new Node(this, endPoint, _settings.NodeSettings);
                    AddNode(node);
                }
            }

            foreach (var node in _nodes.ToList())
            {
                if (!config.Members.Contains(node.EndPoint))
                {
                    RemoveNode(node);
                }
            }
        }

        private async Task BackgroundTask()
        {
            var cancellationToken = _backgroundTaskCancellationTokenSource.Token;
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var newNodeInfo = await _nodeInfoChangedQueue.DequeueAsync(); // TODO: add timeout and cancellationToken to DequeueAsync
                    var clusterInfoChanged = false;
                    ReplicaSetInfo oldClusterInfo = null;
                    ReplicaSetInfo newClusterInfo = null;
                    TaskCompletionSource<bool> oldInfoChangedTaskCompletionSource = null;

                    lock (_lock)
                    {
                        newClusterInfo = _info.WithNodeInfo(newNodeInfo);
                        if (!_info.Equals(newClusterInfo))
                        {
                            clusterInfoChanged = true;
                            oldClusterInfo = _info;
                            oldInfoChangedTaskCompletionSource = _infoChangedTaskCompletionSource;

                            AddOrRemoveNodes(newClusterInfo.Config);

                            _info = newClusterInfo.WithRevision(oldClusterInfo.Revision + 1);
                            _infoChangedTaskCompletionSource = new TaskCompletionSource<bool>();
                            _primary = _nodes
                                .Where(n => n.EndPoint == newClusterInfo.Config.Primary)
                                .Select(n => new WritableNode(n))
                                .FirstOrDefault();
                        }
                    }

                    if (clusterInfoChanged)
                    {
                        oldInfoChangedTaskCompletionSource.TrySetResult(true);
                        OnInfoChanged(oldClusterInfo, newClusterInfo);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // ignore TaskCanceledException
            }
        }

        private Node ChooseNode(ReplicaSetInfo info, ReadPreference readPreference)
        {
            var endPoint = ReadPreferenceNodeSelector.ChooseNode(info, readPreference);
            if (endPoint != null)
            {
                lock (_lock)
                {
                    return _nodes.FirstOrDefault(n => n.EndPoint == endPoint);
                }
            }

            return null;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    _backgroundTaskCancellationTokenSource.Cancel();
                    foreach (var node in _nodes)
                    {
                        node.Dispose();
                    }
                    _disposed = true;
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

                ReplicaSetInfo info;
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

                await infoChangedTask.WithTimeout(slidingTimeout, cancellationToken);
            }
        }

        public async Task<IReadableNode> GetReadableNodeAsync(ReadPreference readPreference, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            var slidingTimeout = new SlidingTimeout(timeout);
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ReplicaSetInfo info;
                Task infoChangedTask;
                lock (_lock)
                {
                    info = _info;
                    infoChangedTask = _infoChangedTaskCompletionSource.Task;
                }

                var node = ChooseNode(info, readPreference);
                if (node != null)
                {
                    return new ReadableNode(node);
                }

                await infoChangedTask.WithTimeout(slidingTimeout, cancellationToken);
            }
        }

        public async Task<IWritableNode> GetWritableNodeAsync(TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            var slidingTimeout = new SlidingTimeout(timeout);
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                IWritableNode primary;
                Task infoChangedTask;
                lock (_lock)
                {
                    primary = _primary;
                    infoChangedTask = _infoChangedTaskCompletionSource.Task;
                }

                if (primary != null)
                {
                    return primary;
                }

                await infoChangedTask.WithTimeout(slidingTimeout, cancellationToken);
            }
        }

        private void NodeInfoChangedHandler(object sender, EventArgs args)
        {
            var node = (INode)sender;
            _nodeInfoChangedQueue.Enqueue(node.Info);
        }

        private void OnInfoChanged(ReplicaSetInfo oldInfo, ReplicaSetInfo newInfo)
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

        private void RemoveNode(Node node)
        {
            var endPoint = node.EndPoint;
            node.InfoChanged -= NodeInfoChangedHandler;
            _nodes.Remove(node);
            node.Dispose();

            var clusterListener = _settings.ClusterListener;
            if (clusterListener != null)
            {
                var args = new NodeRemovedEventArgs(endPoint);
                clusterListener.NodeRemoved(args);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ReplicaSet");
            }
        }
    }
}
