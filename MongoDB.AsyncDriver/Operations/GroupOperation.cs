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
    public class GroupOperation : IReadOperation<IEnumerable<BsonDocument>>
    {
        // fields
        private readonly string _collectionName;
        private readonly string _databaseName;
        private readonly BsonJavaScript _finalizeFunction;
        private readonly BsonDocument _initial;
        private readonly BsonDocument _key;
        private readonly BsonJavaScript _keyFunction;
        private readonly BsonDocument _query;
        private readonly BsonJavaScript _reduceFunction;

        // constructors
        public GroupOperation(string databaseName, string collectionName, BsonDocument key, BsonDocument initial, BsonJavaScript reduceFunction, BsonDocument query = null)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _key = key;
            _initial = initial;
            _reduceFunction = reduceFunction;
            _query = query;
        }

        public GroupOperation(string databaseName, string collectionName, BsonJavaScript keyFunction, BsonDocument initial, BsonJavaScript reduceFunction, BsonDocument query = null)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _keyFunction = keyFunction;
            _initial = initial;
            _reduceFunction = reduceFunction;
            _query = query;
        }

        private GroupOperation(
            string collectionName,
            string databaseName,
            BsonJavaScript finalizeFunction,
            BsonDocument initial,
            BsonDocument key,
            BsonJavaScript keyFunction,
            BsonDocument query,
            BsonJavaScript reduceFunction)
        {
            _collectionName = collectionName;
            _databaseName = databaseName;
            _finalizeFunction = finalizeFunction;
            _initial = initial;
            _key = key;
            _keyFunction = keyFunction;
            _query = query;
            _reduceFunction = reduceFunction;
        }

        // properties
        public string CollectionName
        {
            get { return _collectionName; }
        }

        public string DatabaseName
        {
            get { return _databaseName; }
        }

        public BsonJavaScript FinalizeFunction
        {
            get { return _finalizeFunction; }
        }

        public BsonDocument Initial
        {
            get { return _initial; }
        }

        public BsonDocument Key
        {
            get { return _key; }
        }

        public BsonJavaScript KeyFunction
        {
            get { return _keyFunction; }
        }

        public BsonDocument Query
        {
            get { return _query; }
        }

        public BsonJavaScript ReduceFunction
        {
            get { return _reduceFunction; }
        }

        // methods
        public BsonDocument CreateCommand()
        {
            return new BsonDocument
            {
                { "group", new BsonDocument
                    {
                        { "ns", _collectionName },
                        { "key", _key, _key != null },
                        { "$keyf", _keyFunction, _keyFunction != null },
                        { "$reduce", _reduceFunction },
                        { "initial", _initial },
                        { "cond", _query, _query != null },
                        { "finalize", _finalizeFunction, _finalizeFunction != null }
                    }
                },
            };
        }

        public async Task<IEnumerable<BsonDocument>> ExecuteAsync(IReadableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var document = await ExecuteCommandAsync(connection, timeout, cancellationToken);
            return document["retval"].AsBsonArray().Cast<BsonDocument>();
        }

        public async Task<BsonDocument> ExecuteCommandAsync(IReadableConnection connection, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = CreateCommand();
            var operation = new ReadCommandOperation(_databaseName, command);
            return await operation.ExecuteAsync(connection, timeout, cancellationToken);
        }

        public GroupOperation WithCollectionName(string value)
        {
            return (_collectionName == value) ? this : new Builder(this) { _collectionName = value }.Build();
        }

        public GroupOperation WithDatabaseName(string value)
        {
            return (_databaseName == value) ? this : new Builder(this) { _databaseName = value }.Build();
        }

        public GroupOperation WithFinalizeFunction(BsonJavaScript value)
        {
            return object.ReferenceEquals(_finalizeFunction, value) ? this : new Builder(this) { _finalizeFunction = value }.Build();
        }

         public GroupOperation WithInitial(BsonDocument value)
        {
            return object.ReferenceEquals(_initial, value) ? this : new Builder(this) { _initial = value }.Build();
        }

        public GroupOperation WithKey(BsonDocument value)
        {
            return object.ReferenceEquals(_key, value) ? this : new Builder(this) { _key = value, _keyFunction = null }.Build();
        }

        public GroupOperation WithKeyFunction(BsonJavaScript value)
        {
            return object.ReferenceEquals(_keyFunction, value) ? this : new Builder(this) { _key = null, _keyFunction = value }.Build();
        }
        
        public GroupOperation WithQuery(BsonDocument value)
        {
            return object.ReferenceEquals(_query, value) ? this : new Builder(this) { _query = value }.Build();
        }

        public GroupOperation WithReduceFunction(BsonJavaScript value)
        {
            return object.ReferenceEquals(_reduceFunction, value) ? this : new Builder(this) { _reduceFunction = value }.Build();
        }

        // nested types
        private struct Builder
        {       
            // fields
            public string _collectionName;
            public string _databaseName;
            public BsonJavaScript _finalizeFunction;
            public BsonDocument _initial;
            public BsonDocument _key;
            public BsonJavaScript _keyFunction;
            public BsonDocument _query;
            public BsonJavaScript _reduceFunction;

            // constructors
            public Builder(GroupOperation other)
            {
                _collectionName = other.CollectionName;
                _databaseName = other.DatabaseName;
                _finalizeFunction = other.FinalizeFunction;
                _initial = other.Initial;
                _key = other.Key;
                _keyFunction = other.KeyFunction;
                _query = other.Query;
                _reduceFunction = other.ReduceFunction;
            }

            // methods
            public GroupOperation Build()
            {
                return new GroupOperation(
                    _collectionName,
                    _databaseName,
                    _finalizeFunction,
                    _initial,
                    _key,
                    _keyFunction,
                    _query,
                    _reduceFunction);
            }
        }
    }
}
