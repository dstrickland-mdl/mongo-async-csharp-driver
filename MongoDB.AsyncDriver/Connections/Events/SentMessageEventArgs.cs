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
    public class SentMessageEventArgs
    {
        // fields
        private readonly IConnection _connection;
        private readonly Exception _exception;
        private readonly RequestMessage _message;

        // constructors
        public SentMessageEventArgs(IConnection connection, RequestMessage message, Exception exception)
        {
            _connection = connection;
            _message = message;
            _exception = exception;
        }

        // properties
        public Exception Exception
        {
            get { return _exception; }
        }

        public IConnection Connection
        {
            get { return _connection; }
        }

        public RequestMessage Message
        {
            get { return _message; }
        }
    }
}
