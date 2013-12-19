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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents a factory of BinaryConnections.
    /// </summary>
    public class BinaryConnectionFactory : IConnectionFactory
    {
        // fields
        private readonly IStreamFactory _streamFactory;

        // constructors
        public BinaryConnectionFactory()
        {
            _streamFactory = new TcpStreamFactory();
        }

        public BinaryConnectionFactory(IStreamFactory streamFactory)
        {
            _streamFactory = streamFactory;
        }

        // methods
        public IRootConnection CreateConnection(IRootNode node)
        {
            // postpone creating the stream until BinaryConnection.OpenAsync because some Stream constructors block or throw
            return new BinaryConnection(node, _streamFactory);
        }
    }
}
