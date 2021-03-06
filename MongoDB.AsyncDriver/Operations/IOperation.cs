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
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents a database operation.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface IOperation<TResult>
    {
    }

    /// <summary>
    /// Represents a database read operation.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface IReadOperation<TResult> : IOperation<TResult>
    {
        // methods
        Task<TResult> ExecuteAsync(IReadableConnection connection, TimeSpan timeout, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents a database write operation.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface IWriteOperation<TResult> : IOperation<TResult>
    {
        // methods
        Task<TResult> ExecuteAsync(IWritableConnection connection, TimeSpan timeout, CancellationToken cancellationToken);
    }
}
