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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public class WriteCommandOperation : CommandOperationBase
    {
        // constructors
        public WriteCommandOperation(string databaseName, BsonDocument command)
            : base(command, databaseName)
        {
        }

        // methods
        public Task<BsonDocument> ExecuteAsync(IWritableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            return base.ExecuteAsync(connection, timeout, cancellationToken);
        }

        public WriteCommandOperation WithCommand(BsonDocument value)
        {
            return object.ReferenceEquals(Command, value) ? this : new WriteCommandOperation(DatabaseName, value);
        }

        public WriteCommandOperation WithDatabaseName(string value)
        {
            return object.ReferenceEquals(DatabaseName, value) ? this : new WriteCommandOperation(value, Command);
        }
    }
}
