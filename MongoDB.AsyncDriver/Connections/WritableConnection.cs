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
    /// Represents a connection that can be used for write (as well as read) operations.
    /// </summary>
    public class WritableConnection : WrappedConnection, IWritableConnection
    {
        // fields
        private readonly IWritableNode _writableNode;

        // constructors
        public WritableConnection(IConnection wrapped, IWritableNode writableNode)
            : base(wrapped, writableNode)
        {
            _writableNode = writableNode;
        }

        // properties
        public new IWritableNode Node
        {
            get { return _writableNode; }
        }

        IReadableNode IReadableConnection.Node
        {
            get { return _writableNode; }
        }

        // methods
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _wrapped.Dispose();
            }
        }

        public new IWritableConnection Fork()
        {
            return new WritableConnection(_wrapped.Fork(), _writableNode);
        }

        IReadableConnection IReadableConnection.Fork()
        {
            return Fork();
        }

        protected override IConnection ForkImplementation()
        {
            return Fork();
        }
    }
}
