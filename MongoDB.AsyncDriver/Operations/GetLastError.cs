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
    public static class GetLastError
    {
        public static QueryMessage CreateMessage(string databaseName, WriteConcern writeConcern)
        {
            var command = new BsonDocument 
            {
                { "getLastError", 1 },
                { "w", () => writeConcern.W.ToBsonValue(), writeConcern.W != null },
                { "wtimeout", () => writeConcern.WTimeout.Value.TotalMilliseconds, writeConcern.WTimeout.HasValue },
                { "fsync", () => writeConcern.FSync.Value, writeConcern.FSync.HasValue },
                { "j", () => writeConcern.Journal.Value, writeConcern.Journal.HasValue }
            };

            return new QueryMessage(
               RequestMessage.GetNextRequestId(),
               databaseName,
               "$cmd",
               command,
               null,
               0,
               -1,
               true,
               false,
               false);
        }

        public static BsonDocument ProcessReply(ReplyMessage<BsonDocument> reply)
        {
            if (reply.NumberReturned == 0)
            {
                throw new GetLastErrorException("GetLastError reply had no documents.");
            }
            if (reply.NumberReturned > 1)
            {
                throw new GetLastErrorException("GetLastError reply had more than one document.");
            }
            if (reply.QueryFailure)
            {
                throw new GetLastErrorException("GetLastError reply had QueryFailure flag set.", reply.QueryFailureDocument);
            }

            var result = reply.Documents.Single();
            if (!result.ContainsName("ok"))
            {
                throw new GetLastErrorException("GetLastError result has no ok field.", result);
            }
            if (!result["ok"].ToBoolean())
            {
                throw new GetLastErrorException("GetLastError result had ok 0.", result);
            }
            if (result["err"] != BsonNull.Instance)
            {
                var message = string.Format("GetLastError detected an error: {0}.", result);
                throw new GetLastErrorException(message, result);
            }

            return result;
        }
    }
}
