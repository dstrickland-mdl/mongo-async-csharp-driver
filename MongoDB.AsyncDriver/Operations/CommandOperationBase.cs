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

namespace MongoDB.AsyncDriver
{
    public abstract class CommandOperationBase : IOperation<BsonDocument>
    {
        // fields
        private readonly BsonDocument _command;
        private readonly string _databaseName;

        // constructors
        protected CommandOperationBase(
            BsonDocument command,
            string databaseName)
        {
            _command = command;
            _databaseName = databaseName;
        }

        // properties
        public BsonDocument Command
        {
            get { return _command; }
        }

        public string DatabaseName
        {
            get { return _databaseName; }
        }

        // methods
        private QueryMessage CreateMessage(INode node)
        {
            var slaveOk = !(node is IWritableNode) || (node.Cluster is DirectCluster);
            return new QueryMessage(
                RequestMessage.GetNextRequestId(),
                _databaseName,
                "$cmd",
                _command,
                null,
                0,
                -1,
                slaveOk,
                false,
                false);
        }

        protected async Task<BsonDocument> ExecuteAsync(IConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var slidingTimeout = new SlidingTimeout(timeout);
            var message = CreateMessage(connection.Node);
            await connection.SendMessageAsync(message, slidingTimeout, cancellationToken);
            var reply = await connection.ReceiveMessageAsync<BsonDocument>(message.RequestId, BsonDocumentSerializer.Instance, slidingTimeout, cancellationToken);
            return ProcessReply(reply);
        }

        private BsonDocument ProcessReply(ReplyMessage<BsonDocument> reply)
        {
            if (reply.NumberReturned == 0)
            {
                throw new CommandException("Command reply had no documents.", _command);
            }
            if (reply.NumberReturned > 1)
            {
                throw new CommandException("Command reply had more than one document.", _command);
            }
            if (reply.QueryFailure)
            {
                throw new CommandException("Command reply had QueryFailure flag set.", _command, reply.QueryFailureDocument);
            }

            var result = reply.Documents.Single();
            if (!result["ok"].ToBoolean())
            {
                var message = result.ToString();
                throw new CommandException(message, _command, result);
            }

            return result;
        }
    }
}
