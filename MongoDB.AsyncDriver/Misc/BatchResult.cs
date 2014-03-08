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
    public class BatchResult<T>
    {
        // fields
        private readonly int _batchCount;
        private IReadOnlyList<T> _batchItems;
        private readonly int _batchLength;
        private readonly Batch<T> _nextBatch;

        // constructors
        public BatchResult(int batchCount, int batchLength, IReadOnlyList<T> batchItems, Batch<T> nextBatch)
        {
            _batchCount = batchCount;
            _batchLength = batchLength;
            _batchItems = batchItems;
            _nextBatch = nextBatch;
        }

        // properties
        public int BatchCount
        {
            get { return _batchCount; }
        }

        public IReadOnlyList<T> BatchItems
        {
            get { return _batchItems; }
        }

        public int BatchLength
        {
            get { return _batchLength; }
        }

        public bool IsLast
        {
            get { return _nextBatch == null; }
        }

        public Batch<T> NextBatch
        {
            get { return _nextBatch; }
        }
    }
}
