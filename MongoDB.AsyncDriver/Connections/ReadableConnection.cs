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
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents a connection that can be used for read operations.
    /// </summary>
    public class ReadableConnection : WrappedConnection, IReadableConnection
    {
        // fields
        private readonly IReadableNode _readableNode;

        // constructors
        public ReadableConnection(IConnection wrapped, IReadableNode readableNode)
            : base(wrapped, readableNode)
        {
            _readableNode = readableNode;
        }

        // properties
        public new IReadableNode Node
        {
            get { return _readableNode; }
        }

        // methods
        public new IReadableConnection Fork()
        {
            return new ReadableConnection(_wrapped.Fork(), _readableNode);
        }

        protected override IConnection ForkImplementation()
        {
            return Fork();
        }
    }
}
