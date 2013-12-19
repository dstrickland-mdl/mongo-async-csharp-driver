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
    public class ReceivedMessageEventArgs
    {
        // fields
        private readonly IConnection _connection;
        private readonly Exception _exception;
        private readonly ReplyMessage _reply;
        private ReplyMessage _substituteReply;

        // constructors
        public ReceivedMessageEventArgs(IConnection connection, ReplyMessage reply)
        {
            _connection = connection;
            _reply = reply;
        }

        public ReceivedMessageEventArgs(IConnection connection, Exception exception)
        {
            _connection = connection;
            _exception = exception;
        }

        // properties
        public IConnection Connection
        {
            get { return _connection; }
        }

        public Exception Exception
        {
            get { return _exception; }
        }

        public ReplyMessage Reply
        {
            get { return _reply; }
        }

        public ReplyMessage SubstituteReply
        {
            get { return _substituteReply; }
            set { _substituteReply = value; }
        }
    }
}
