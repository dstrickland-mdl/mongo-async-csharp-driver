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
    public class LazyAsyncEnumerator<TDocument> : IAsyncEnumerator<TDocument>
    {
        // fields
        private IAsyncEnumerator<TDocument> _enumerator;
        private Func<Task<IAsyncEnumerator<TDocument>>> _enumeratorFactory;

        // constructor
        public LazyAsyncEnumerator(Func<Task<IAsyncEnumerator<TDocument>>> enumeratorFactory)
        {
            _enumeratorFactory = enumeratorFactory;
        }

        // properties
        public TDocument Current
        {
            get
            {
                if (_enumerator == null)
                {
                    throw new InvalidOperationException("Enumeration has not started. Call MoveNextAsync.");
                }
                return _enumerator.Current;
            }
        }

        // methods
        public void Dispose()
        {
            if (_enumerator != null)
            {
                _enumerator.Dispose();
            }
        }

        public async Task<bool> MoveNextAsync()
        {
            if (_enumerator == null)
            {
                _enumerator = await _enumeratorFactory();
            }
            return await _enumerator.MoveNextAsync();
        }
    }
}
