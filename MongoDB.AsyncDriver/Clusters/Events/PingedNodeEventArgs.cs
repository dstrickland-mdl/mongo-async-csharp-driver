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
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public class PingedNodeEventArgs
    {
        // fields
        private readonly BsonDocument _buildInfoResult;
        private readonly BsonDocument _isMasterResult;
        private readonly TimeSpan _pingTime;
        private readonly IConnection _connection;

        // constructors
        public PingedNodeEventArgs(IConnection connection, TimeSpan pingTime, BsonDocument isMasterResult, BsonDocument buildInfoResult)
        {
            _connection = connection;
            _pingTime = pingTime;
            _isMasterResult = isMasterResult;
            _buildInfoResult = buildInfoResult;
        }

        // properties
        public BsonDocument BuildInfoResult
        {
            get { return _buildInfoResult; }
        }

        public IConnection Connection
        {
            get { return _connection; }
        }

        public BsonDocument IsMasterResult
        {
            get { return _isMasterResult; }
        }

        public TimeSpan PingTime
        {
            get { return _pingTime; }
        }
    }
}
