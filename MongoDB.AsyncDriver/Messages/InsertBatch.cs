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
    public abstract class InsertBatch<TDocument>
    {
        // fields
        private int _batchCount;
        private int _batchLength;
        private IReadOnlyList<TDocument> _batchDocuments;
        private readonly IEnumerator<TDocument> _enumerator;
        private InsertBatch<TDocument> _nextBatch;

        // constructors
        protected InsertBatch(IEnumerator<TDocument> enumerator)
        {
            _enumerator = enumerator;
        }

        // properties
        public int BatchCount
        {
            get { return _batchCount; }
        }

        public int BatchLength
        {
            get { return _batchLength; }
        }

        public IReadOnlyList<TDocument> BatchDocuments
        {
            get { return _batchDocuments; }
        }

        public virtual bool CanBeSplit
        {
            get { return true; }
        }

        public IEnumerator<TDocument> Enumerator
        {
            get { return _enumerator; }
        }

        public InsertBatch<TDocument> NextBatch
        {
            get { return _nextBatch; }
        }

        // methods
        public void SetBatchResults(int batchCount, int batchLength, IReadOnlyList<TDocument> batchDocuments, InsertBatch<TDocument> nextBatch)
        {
            _batchCount = batchCount;
            _batchLength = batchLength;
            _batchDocuments = batchDocuments;
            _nextBatch = nextBatch;
        }

        // nested types
        public class FirstBatch : InsertBatch<TDocument>
        {
            // fields
            private readonly bool _canBeSplit;

            // constructors
            public FirstBatch(IEnumerator<TDocument> enumerator, bool canBeSplit = true)
                : base(enumerator)
            {
                _canBeSplit = canBeSplit;
            }

            // properties
            public override bool CanBeSplit
            {
                get { return _canBeSplit; }
            }
        }
    }
}
