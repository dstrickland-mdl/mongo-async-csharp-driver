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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents a node in a MongoDB cluster.
    /// </summary>
    public class Node : IRootNode
    {
        // fields
        private readonly CancellationTokenSource _backgroundTaskCancellationTokenSource = new CancellationTokenSource();
        private readonly ICluster _cluster;
        private readonly IDedicatedConnectionPool _dedicatedConnectionPool;
        private bool _disposed;
        private readonly DnsEndPoint _endPoint;
        private NodeInfo _info;
        private TaskCompletionSource<bool> _infoChangedTaskCompletionSource = new TaskCompletionSource<bool>();
        private object _lock = new object();
        private InterruptibleDelay _pingDelay = new InterruptibleDelay(TimeSpan.Zero);
        private readonly NodeSettings _settings;
        private readonly IConnectionPool _sharedConnectionPool;

        // events
        public event EventHandler InfoChanged;

        // constructors
        internal Node(ICluster cluster, DnsEndPoint endPoint, NodeSettings settings)
        {
            _cluster = cluster;
            _endPoint = endPoint;
            _settings = settings;
            _info = NodeInfo.Create(endPoint, NodeState.Disconnected, null, null);
            _dedicatedConnectionPool = settings.ConnectionPoolFactory.CreateDedicatedConnectionPool(this);
            _sharedConnectionPool = settings.ConnectionPoolFactory.CreateSharedConnectionPool(this) ?? _dedicatedConnectionPool;
        }

        // properties
        public ICluster Cluster
        {
            get { return _cluster; }
        }

        public DnsEndPoint EndPoint
        {
            get { return _endPoint; }
        }

        public NodeInfo Info
        {
            get
            {
                lock (_lock)
                {
                    return _info;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                NodeInfo info;
                lock (_lock)
                {
                    info = _info;
                }

                switch (info.Type)
                {
                    case NodeType.StandAlone:
                    case NodeType.Primary:
                    case NodeType.ShardRouter:
                        return false;

                    default:
                        return true;
                }
            }
        }

        // methods
        private void CheckIfInfoChanged(NodeInfo newInfo)
        {
            var infoChanged = false;
            NodeInfo oldInfo = null;
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
                    _backgroundTaskCancellationTokenSource.Cancel();
                    _dedicatedConnectionPool.Dispose();
                    _sharedConnectionPool.Dispose();
                    _disposed = true;
                }
            }
        }

        public async Task<IWritableConnection> GetConnectionAsync(TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var connection = await _sharedConnectionPool.AcquireConnectionAsync(timeout, cancellationToken);
            return new WritableConnection(connection, this);
        }

        async Task<IReadableConnection> IReadableNode.GetConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            var connection = await _sharedConnectionPool.AcquireConnectionAsync(timeout, cancellationToken);
            return new ReadableConnection(connection, this);
        }

        public async Task<IWritableConnection> GetDedicatedConnectionAsync(TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var connection = await _dedicatedConnectionPool.AcquireConnectionAsync(timeout, cancellationToken);
            return new WritableConnection(connection, this);
        }

        async Task<IReadableConnection> IReadableNode.GetDedicatedConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            var connection = await _dedicatedConnectionPool.AcquireConnectionAsync(timeout, cancellationToken);
            return new ReadableConnection(connection, this);
        }

        public async Task<NodeInfo> GetInfoAsync(int minimumRevision, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var slidingTimeout = new SlidingTimeout(timeout);
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                slidingTimeout.ThrowIfExpired();

                NodeInfo info;
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

                await infoChangedTask;
            }
        }

        private void OnInfoChanged(NodeInfo oldInfo, NodeInfo newInfo)
        {
            var clusterListener = _cluster.Settings.ClusterListener;
            if (clusterListener != null)
            {
                var args = new NodeInfoChangedEventArgs(this, oldInfo, newInfo);
                clusterListener.NodeInfoChanged(args);
            }

            var handler = InfoChanged;
            if (handler != null)
            {
                try { handler(this, EventArgs.Empty); }
                catch { } // ignore exceptions
            }
        }

        private async Task<PingInfo> PingAsync(IReadableConnection connection, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var slidingTimeout = new SlidingTimeout(timeout);

            var stopwatch = Stopwatch.StartNew();
            await new ReadCommandOperation("admin", new BsonDocument("ping", 1)).ExecuteAsync(connection, slidingTimeout, cancellationToken);
            stopwatch.Stop();
            var pingTime = stopwatch.Elapsed;

            var isMasterResult = await new ReadCommandOperation("admin", new BsonDocument("isMaster", 1)).ExecuteAsync(connection, slidingTimeout, cancellationToken);
            var buildInfoResult = await new ReadCommandOperation("admin", new BsonDocument("buildInfo", 1)).ExecuteAsync(connection, slidingTimeout, cancellationToken);

            return new PingInfo { Connection = connection, PingTime = pingTime, IsMasterResult = isMasterResult, BuildInfoResult = buildInfoResult };
        }

        public async Task PingBackgroundTask()
        {
            var cancellationToken = _backgroundTaskCancellationTokenSource.Token;
            try
            {
                IReadableConnection connection = null;
                try
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var pingInfo = await PingWithRetryAsync(connection, _settings.PingTimeout, cancellationToken);
                        connection = pingInfo.Connection; // PingWithRetryAsync creates (or recreates) connections as necessary

                        var nodeInfo = ToNodeInfo(pingInfo);                       
                        CheckIfInfoChanged(nodeInfo);

                        var pingDelay = new InterruptibleDelay(_settings.PingInterval);
                        lock (_lock)
                        {
                            _pingDelay = pingDelay;
                        }
                        await pingDelay.Task;
                    }
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Dispose();
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // ignore TaskCanceledException
            }
        }

        private async Task<PingInfo> PingWithRetryAsync(IReadableConnection connection, TimeSpan timeout, CancellationToken cancellationToken)
        {
            PingInfo pingInfo = null;

            var clusterListener = _cluster.Settings.ClusterListener;
            if (clusterListener != null)
            {
                var args = new PingingNodeEventArgs(_endPoint);
                clusterListener.PingingNode(args);
            }

            // if the first attempt fails try a second time with a new connection
            for (var attempt = 0; pingInfo == null && attempt < 2; attempt++)
            {
                try
                {
                    var slidingTimeout = new SlidingTimeout(timeout);
                    if (connection == null)
                    {
                        var unpooledConnection = await _dedicatedConnectionPool.CreateUnpooledConnectionAsync(slidingTimeout, cancellationToken);
                        connection = new ReadableConnection(unpooledConnection, this);
                    }

                    pingInfo = await PingAsync(connection, slidingTimeout, cancellationToken);
                }
                catch
                {
                    // TODO: log the exception?
                    if (connection != null)
                    {
                        connection.Dispose();
                        connection = null;
                    }
                }
            }

            if (clusterListener != null)
            {
                PingedNodeEventArgs args;
                if (pingInfo == null)
                {
                    args = new PingedNodeEventArgs(connection, TimeSpan.Zero, null, null);
                }
                else
                {
                    args = new PingedNodeEventArgs(connection, pingInfo.PingTime, pingInfo.IsMasterResult, pingInfo.BuildInfoResult);
                }
                clusterListener.PingedNode(args);
            }

            return pingInfo ?? new PingInfo { Connection = null };
        }

        internal void StartBackgroundTasks()
        {
            PingBackgroundTask().LogUnobservedExceptions();
        }

        private NodeInfo ToNodeInfo(PingInfo pingInfo)
        {
            if (pingInfo.Connection == null)
            {
                return NodeInfo.Create(_endPoint, NodeState.Disconnected, null, null);
            }
            else
            {
                pingInfo.IsMasterResult.Remove("localTime"); // remove localTime from IsMasterResult because it changes every time
                return NodeInfo.Create(_endPoint, NodeState.Connected, pingInfo.IsMasterResult, pingInfo.BuildInfoResult);
            }

        }

        // nested types
        private class PingInfo
        {
            public IReadableConnection Connection;
            public TimeSpan PingTime;
            public BsonDocument IsMasterResult;
            public BsonDocument BuildInfoResult;
        }
    }
}
