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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.AsyncDriver;

namespace ConsoleTestApplication
{
    public class Logger : IClusterListener, IMessageListener
    {
        #region static
        // static fields
        private static long __id;
        #endregion

        // fields
        private readonly TextWriter[] _destinations;

        // constructors
        public Logger(params TextWriter[] destinations)
        {
            _destinations = destinations;
        }

        // methods
        public void ClusterInfoChanged(ClusterInfoChangedEventArgs args)
        {
        }

        private void LogMessage(string eventType, IConnection connection, MongoDBMessage message)
        {
            var node = connection.Node;
            using (var stringWriter = new StringWriter())
            {
                var jsonWriter = new JsonWriter(stringWriter);
                var encoderFactory = new JsonMessageEncoderFactory(jsonWriter);
                message.GetEncoder(encoderFactory).WriteMessage(message);
                var jsonMessage = stringWriter.ToString();
                var logMessage = string.Format("Node: '{0}', ConnectionId: {1}, Message: {2}", ToString(node), connection.ConnectionId, jsonMessage);
                LogMessage(eventType, logMessage);
            }
        }

        private void LogMessage(string eventType, string message)
        {
            var id = Interlocked.Increment(ref __id);
            var timestampedMessage = string.Format("{{ _id: {0}, ts: ISODate('{1}'), eventType: '{2}', {3} }}", id, DateTime.Now.ToString("s"), eventType, message);

            foreach (var destination in _destinations)
            {
                destination.WriteLine(timestampedMessage);
                destination.Flush();
            }
        }
        public void NodeAdded(NodeAddedEventArgs args)
        {
            var node = args.Node;
            var logMessage = string.Format("Node: '{0}'", ToString(node));
            LogMessage("NodeAdded", logMessage);
        }

        public void NodeInfoChanged(NodeInfoChangedEventArgs args)
        {
            var node = args.NewNodeInfo;
            var logMessage = string.Format("Node: '{0}', OldInfo: {1}, NewInfo: '{2}'", ToString(args.Node), args.OldNodeInfo, args.NewNodeInfo);
            LogMessage("NodeInfoChanged", logMessage);
        }

        public void NodeRemoved(NodeRemovedEventArgs args)
        {
        }

        public void PingedNode(PingedNodeEventArgs args)
        {
            var connection = args.Connection;
            var node = (connection == null) ? null : connection.Node;
            var connectionId = (connection == null) ? 0 : connection.ConnectionId;
            var logMessage = string.Format("Node: '{0}', ConnectionId: {1}, PingTime: {2}, IsMasterResult: {3}, BuildInfoResult: {4}", ToString(node), connectionId, args.PingTime.TotalMilliseconds, args.IsMasterResult, args.BuildInfoResult);
            LogMessage("PingedNode", logMessage);
        }

        public void PingingNode(PingingNodeEventArgs args)
        {
            var logMessage = string.Format("Node: '{0}'", DnsEndPointParser.ToString(args.EndPoint));
            LogMessage("PingingNode", logMessage);
        }

        public Task ReceivedMessageAsync(ReceivedMessageEventArgs args)
        {
            LogMessage("ReceivedMessage", args.Connection, args.Reply);
            return Task.FromResult(true);
        }

        public Task SendingMessageAsync(SendingMessageEventArgs args)
        {
            LogMessage("SendingMessage", args.Connection, args.Message);

            var queryMessage = args.Message as QueryMessage;
            if (queryMessage != null)
            {
                if (queryMessage.DatabaseName == "admin" && queryMessage.CollectionName == "timeofday")
                {
                    var documents = new List<BsonDocument> { new BsonDocument { { "_id", 1 }, { "time", DateTime.UtcNow } } };
                    var replyMessage = new ReplyMessage<BsonDocument>(
                        0,
                        false,
                        documents,
                        1,
                        false,
                        null,
                        -1,
                        queryMessage.RequestId,
                        BsonDocumentSerializer.Instance,
                        0);
                    args.SubstituteReply = replyMessage;
                }
            }

            return Task.FromResult(true);
        }

        public Task SentMessageAsync(SentMessageEventArgs args)
        {
            var connection = args.Connection;
            var node = connection.Node;
            var logMessage = string.Format("Node: '{0}', ConnectionId: {1}, Sent: {{ requestId : {1} }}", ToString(node), connection.ConnectionId, args.Message.RequestId);
            LogMessage("SentMessage", logMessage);
            return Task.FromResult(true);
        }

        private string ToString(INode node)
        {
            return (node == null) ? "" : DnsEndPointParser.ToString(node.EndPoint);
        }
    }
}
