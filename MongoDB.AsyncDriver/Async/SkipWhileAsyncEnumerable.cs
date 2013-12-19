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
    public class SkipWhileAsyncEnumerable<TSource> : IAsyncEnumerable<TSource>
    {
        // fields
        private readonly IAsyncEnumerable<TSource> _source;
        private readonly Func<TSource, int, bool> _predicate;

        // constructors
        public SkipWhileAsyncEnumerable(IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            _source = source;
            _predicate = (element, index) => predicate(element);
        }

        public SkipWhileAsyncEnumerable(IAsyncEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        // methods
        public IAsyncEnumerator<TSource> GetAsyncEnumerator()
        {
            return new Enumerator(_source.GetAsyncEnumerator(), _predicate);
        }

        // nested classes
        private class Enumerator : IAsyncEnumerator<TSource>
        {
            // fields
            private readonly IAsyncEnumerator<TSource> _source;
            private readonly Func<TSource, int, bool> _predicate;
            private int _index;
            private bool _skipping = true;

            // constructors
            public Enumerator(IAsyncEnumerator<TSource> source, Func<TSource, int, bool> predicate)
            {
                _source = source;
                _predicate = predicate;
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
                while (_skipping)
                {
                    if (await _source.MoveNextAsync())
                    {
                        var element = _source.Current;
                        if (!_predicate(element, _index))
                        {
                            _skipping = false;
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    _index++;
                }
                return await _source.MoveNextAsync();
            }
        }
    }
}
