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
    /// Represents a dedicated connection.
    /// </summary>
    internal class DedicatedConnection : PooledConnection
    {
        // fields
        private int _referenceCount;

        // constructors
        public DedicatedConnection(IRootConnection wrapped)
            : base(wrapped)
        {
            _referenceCount = 1;
        }

        // methods
        public int DecrementReferenceCount()
        {
            return Interlocked.Decrement(ref _referenceCount);
        }

        public override IConnection Fork()
        {
            Interlocked.Increment(ref _referenceCount);
            return this;
        }
    }
}
