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
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public class DocumentCursor<TDocument> : IAsyncEnumerator<TDocument>
    {
        // fields
        private IEnumerator<TDocument> _batch;
        private readonly BatchCursor<TDocument> _batchCursor;
        private long _count;
        private bool _disposed;
        private bool _done;
        private readonly long? _limit;

        // constructors
        public DocumentCursor(BatchCursor<TDocument> batchCursor)
            : this(batchCursor, null)
        {
        }

        public DocumentCursor(BatchCursor<TDocument> batchCursor, long? limit)
        {
            _batchCursor = batchCursor;
            _limit = limit;
        }

        // properties
        public TDocument Current
        {
            get
            {
                if (_disposed) { throw new ObjectDisposedException("DocumentAsyncEnumerator<TDocument>"); }
                if (_batch == null)
                {
                    if (_done)
                    {
                        throw new InvalidOperationException("Enumeration already finished.");
                    }
                    else
                    {
                        throw new InvalidOperationException("Enumeration has not started. Call MoveNextAsync.");
                    }
                }
                return _batch.Current;
            }
        }

        // methods
        public void Dispose()
        {
            if (!_disposed)
            {
                if (_batch != null)
                {
                    _batch.Dispose();
                }
                _batchCursor.Dispose();
                _disposed = true;
            }
        }

        public async Task<bool> MoveNextAsync()
        {
            if (_disposed) { throw new ObjectDisposedException("DocumentAsyncEnumerator<TDocument>"); }
            if (_done)
            {
                return false;
            }

            if (_limit.HasValue && _count == _limit.Value)
            {
                _done = true;
                return false;
            }

            while (true)
            {
                if (_batch == null)
                {
                    if (await _batchCursor.MoveNextAsync())
                    {
                        _batch = _batchCursor.Current.GetEnumerator();
                    }
                    else
                    {
                        _done = true;
                        return false;
                    }
                }

                if (_batch.MoveNext())
                {
                    _count++;
                    return true;
                }
                else
                {
                    _batch = null;
                }
            }
        }
    }
}
