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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents a connection using the binary wire protocol over a binary stream.
    /// </summary>
    public class BinaryConnection : IRootConnection
    {
        // fields
        private readonly CancellationTokenSource _backgroundTaskCancellationTokenSource;
        private readonly CancellationToken _backgroundTaskCancellationToken;
        private int _connectionId;
        private bool _disposed;
        private readonly AsyncDropbox<int, InboundDropboxEntry> _inboundDropbox = new AsyncDropbox<int, InboundDropboxEntry>();
        private readonly IMessageListener _messageListener;
        private readonly IRootNode _node;
        private readonly AsyncQueue<OutboundQueueEntry> _outboundQueue = new AsyncQueue<OutboundQueueEntry>();
        private Stream _stream;
        private readonly IStreamFactory _streamFactory;

        // constructors
        public BinaryConnection(IRootNode node, IStreamFactory streamFactory)
        {
            _node = node;
            _streamFactory = streamFactory;
            _backgroundTaskCancellationTokenSource = new CancellationTokenSource();
            _backgroundTaskCancellationToken = _backgroundTaskCancellationTokenSource.Token;
            _messageListener = node.Cluster.Settings.MessageListener;
            // postpone creating the Stream until OpenAsync because some Streams block or throw in the constructor
        }

        // properties
        public int ConnectionId
        {
            get { return _connectionId; }
        }

        public IWritableNode Node
        {
            get { return _node; }
        }

        INode IConnection.Node
        {
            get { return _node; }
        }

        IReadableNode IReadableConnection.Node
        {
            get { return _node; }
        }

        // methods
        public void ConnectionFailed(Exception ex)
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _backgroundTaskCancellationTokenSource.Cancel();
                }
                _disposed = true;
            }
        }

        public IWritableConnection Fork()
        {
            throw new NotSupportedException();
        }

        IConnection IConnection.Fork()
        {
            throw new NotSupportedException();
        }

        IReadableConnection IReadableConnection.Fork()
        {
            throw new NotSupportedException();
        }

        private async Task OnSentMessagesAsync(List<RequestMessage> messages, Exception ex)
        {
            if (_messageListener != null)
            {
                foreach (var message in messages)
                {
                    var args = new SentMessageEventArgs(this, message, ex);
                    await _messageListener.SentMessageAsync(args);
                }
            }
        }

        public async Task OpenAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            _stream = await _streamFactory.CreateStreamAsync(_node.EndPoint, timeout, cancellationToken);
            StartBackgroundTasks();
        }

        private async Task ReceiveBackgroundTask()
        {
            try
            {
                while (true)
                {
                    _backgroundTaskCancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var lengthBytes = new byte[4];
                        await _stream.FillBufferAsync(lengthBytes, 0, 4, _backgroundTaskCancellationToken);
                        var length = BitConverter.ToInt32(lengthBytes, 0);
                        var buffer = BufferPool.Default.Acquire(length);
                        System.Buffer.BlockCopy(lengthBytes, 0, buffer.Bytes, 0, 4);
                        await _stream.FillBufferAsync(buffer.Bytes, 4, length - 4, _backgroundTaskCancellationToken);
                        var responseTo = BitConverter.ToInt32(buffer.Bytes, 8);
                        _inboundDropbox.Post(responseTo, new InboundDropboxEntry(buffer));
                    }
                    catch (Exception)
                    {
                        // TODO: what?
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // ignore TaskCanceledException
            }
        }

        public async Task<ReplyMessage<TDocument>> ReceiveMessageAsync<TDocument>(int responseTo, IBsonSerializer<TDocument> serializer, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var dropboxEntry = await _inboundDropbox.ReceiveAsync(responseTo, timeout, cancellationToken);

            ReplyMessage<TDocument> reply;
            if (dropboxEntry.Buffer != null)
            {
                using (var buffer = dropboxEntry.Buffer)
                using (var stream = new MemoryStream(buffer.Bytes, 0, buffer.Length, false))
                {
                    var encoderFactory = new BinaryMessageEncoderFactory(stream);
                    var encoder = encoderFactory.GetReplyMessageEncoder<TDocument>(serializer);
                    reply = encoder.ReadMessage();
                }
            }
            else
            {
                reply = (ReplyMessage<TDocument>)dropboxEntry.Reply;
            }

            ReplyMessage<TDocument> substituteReply = null;
            if (_messageListener != null)
            {
                var args = new ReceivedMessageEventArgs(this, reply);
                await _messageListener.ReceivedMessageAsync(args);
                substituteReply = (ReplyMessage<TDocument>)args.SubstituteReply;
            }

            return substituteReply ?? reply;
        }

        private async Task SendBackgroundTask()
        {
            try
            {
                while (true)
                {
                    _backgroundTaskCancellationToken.ThrowIfCancellationRequested();

                    var entry = await _outboundQueue.DequeueAsync();
                    using (var buffer = entry.Buffer)
                    {
                        try
                        {
                            await _stream.WriteAsync(buffer.Bytes, 0, buffer.Length, _backgroundTaskCancellationToken);
                            entry.TaskCompletionSource.TrySetResult(true);
                        }
                        catch (Exception ex)
                        {
                            entry.TaskCompletionSource.TrySetException(ex);
                            ConnectionFailed(ex);
                            throw;
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // ignore TaskCanceledException
            }
        }

        public async Task SendMessagesAsync(IEnumerable<RequestMessage> messages, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var sentMessages = new List<RequestMessage>();
            var substituteReplies = new Dictionary<int, ReplyMessage>();

            IBuffer buffer;
            using (var stream = new BufferStream(BufferPool.Default, 1024))
            {
                var encoderFactory = new BinaryMessageEncoderFactory(stream);
                foreach (var message in messages)
                {
                    RequestMessage substituteMessage = null;
                    ReplyMessage substituteReply = null;
                    if (_messageListener != null)
                    {
                        var args = new SendingMessageEventArgs(this, message);
                        await _messageListener.SendingMessageAsync(args);
                        substituteMessage = args.SubstituteMessage;
                        substituteReply = args.SubstituteReply;
                    }

                    var actualMessage = substituteMessage ?? message;
                    sentMessages.Add(actualMessage);

                    if (substituteReply == null)
                    {
                        var encoder = actualMessage.GetEncoder(encoderFactory);
                        encoder.WriteMessage(actualMessage);
                    }
                    else
                    {
                        substituteReplies.Add(message.RequestId, substituteReply);
                    }
                }
                buffer = stream.DetachBuffer();
            }

            Exception exception = null;
            try
            {
                using (buffer)
                {
                    var entry = new OutboundQueueEntry(buffer, cancellationToken);
                    _outboundQueue.Enqueue(entry);
                    await entry.Task;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            await OnSentMessagesAsync(sentMessages, exception);
            if (exception != null)
            {
                throw exception;
            }

            foreach (var requestId in substituteReplies.Keys)
            {
                _inboundDropbox.Post(requestId, new InboundDropboxEntry(substituteReplies[requestId]));
            }
        }

        public void SetConnectionId(int connectionId)
        {
            _connectionId = connectionId;
        }

        private void StartBackgroundTasks()
        {
            SendBackgroundTask().LogUnobservedExceptions();
            ReceiveBackgroundTask().LogUnobservedExceptions();
        }

        // nested classes
        private class InboundDropboxEntry
        {
            // fields
            private readonly IBuffer _buffer;
            private readonly ReplyMessage _reply;

            // constructors
            public InboundDropboxEntry(IBuffer buffer)
            {
                _buffer = buffer;
            }

            public InboundDropboxEntry(ReplyMessage reply)
            {
                _reply = reply;
            }

            // properties
            public IBuffer Buffer
            {
                get { return _buffer; }
            }

            public ReplyMessage Reply
            {
                get { return _reply; }
            }
        }

        private class OutboundQueueEntry
        {
            // fields
            private readonly IBuffer _buffer;
            private readonly CancellationToken _cancellationToken;
            private readonly TaskCompletionSource<bool> _taskCompletionSource;

            // constructors
            public OutboundQueueEntry(IBuffer buffer, CancellationToken cancellationToken)
            {
                _buffer = buffer;
                _cancellationToken = cancellationToken;
                _taskCompletionSource = new TaskCompletionSource<bool>();
            }

            // public properties
            public CancellationToken CancellationToken
            {
                get { return _cancellationToken; }
            }

            public IBuffer Buffer
            {
                get { return _buffer; }
            }

            public Task Task
            {
                get { return _taskCompletionSource.Task; }
            }

            public TaskCompletionSource<bool> TaskCompletionSource
            {
                get { return _taskCompletionSource; }
            }
        }
    }
}
