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
    public class SkipAsyncEnumerable<TSource> : IAsyncEnumerable<TSource>
    {
        // fields
        private readonly IAsyncEnumerable<TSource> _source;
        private readonly int _count;
        
        // constructors
        public SkipAsyncEnumerable(IAsyncEnumerable<TSource> source, int count)
        {
            _source = source;
            _count = count;
        }

        // methods
        public IAsyncEnumerator<TSource> GetAsyncEnumerator()
        {
            return new Enumerator(_source.GetAsyncEnumerator(), _count);
        }

        // nested classes
        private class Enumerator : IAsyncEnumerator<TSource>
        {
            // fields
            private readonly IAsyncEnumerator<TSource> _source;
            private int _count;

            // constructors
            public Enumerator(IAsyncEnumerator<TSource> source, int count)
            {
                _source = source;
                _count = count;
            }

            // properties
            public TSource Current
            {
                get { return _source.Current; }
            }

            // methods
            public void Dispose()
            {
                _source.Dispose();
            }

            public async Task<bool> MoveNextAsync()
            {
                while (_count > 0)
                {
                    if (await _source.MoveNextAsync())
                    {
                        _count--;
                    }
                    else
                    {
                        return false;
                    }
                }
                return await _source.MoveNextAsync();
            }
        }
    }
}
