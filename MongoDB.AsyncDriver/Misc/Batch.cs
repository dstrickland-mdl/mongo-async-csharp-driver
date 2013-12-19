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
    public abstract class Batch<T>
    {
        // fields
        private readonly IEnumerator<T> _enumerator;
        private BatchResult<T> _batchResult;

        // constructors
        protected Batch(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }

        // properties
        public BatchResult<T> BatchResult
        {
            get { return _batchResult; }
            internal set { _batchResult = value; }
        }

        public abstract bool CanBeSplit { get; }

        internal IEnumerator<T> Enumerator
        {
            get { return _enumerator; }
        }

        // nested types
        public class ContinuationBatch<TOverflow> : Batch<T>
        {
            // fields
            private TOverflow _overflow;

            // constructors
            public ContinuationBatch(IEnumerator<T> enumerator, TOverflow overflow)
                : base(enumerator)
            {
                _overflow = overflow;
            }

            // properties
            public override bool CanBeSplit
            {
                get { return true; }
            }

            public TOverflow Overflow
            {
                get { return _overflow; }
                set { _overflow = value; }
            }
        }

        public class FirstBatch : Batch<T>
        {
            // fields
            private readonly bool _canBeSplit;

            // constructors
            public FirstBatch(IEnumerator<T> enumerator, bool canBeSplit = true)
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
