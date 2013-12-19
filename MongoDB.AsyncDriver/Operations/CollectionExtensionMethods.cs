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
    public static class CollectionExtensionMethods
    {
        public static AggregateOperation Aggregate<TDocument>(this Collection<TDocument> collection, IEnumerable<BsonDocument> pipeline)
        {
            return new AggregateOperation(collection.DatabaseName, collection.CollectionName, pipeline);
        }

        public static DeleteCommandOperation Delete<TDocument>(this Collection<TDocument> collection, IEnumerable<DeleteRequest> deletes)
        {
            return new DeleteCommandOperation(collection.DatabaseName, collection.CollectionName, deletes);
        }

        public static DeleteOpcodeOperation DeleteUsingOpcode<TDocument>(this Collection<TDocument> collection, BsonDocument query)
        {
            return new DeleteOpcodeOperation(collection.DatabaseName, collection.CollectionName, query);
        }

        public static DistinctOperation Distinct<TDocument>(this Collection<TDocument> collection, string key, BsonDocument query = null)
        {
            return new DistinctOperation(collection.DatabaseName, collection.CollectionName, key, query);
        }

        public static DropCollectionOperation Drop<TDocument>(this Collection<TDocument> collection)
        {
            return new DropCollectionOperation(collection.DatabaseName, collection.CollectionName);
        }

        public static DropIndexOperation DropIndex<TDocument>(this Collection<TDocument> collection, BsonDocument keys)
        {
            return new DropIndexOperation(collection.DatabaseName, collection.CollectionName, keys);
        }

        public static DropIndexOperation DropIndex<TDocument>(this Collection<TDocument> collection, string indexName)
        {
            return new DropIndexOperation(collection.DatabaseName, collection.CollectionName, indexName);
        }

        public static CollectionExistsOperation Exists<TDocument>(this Collection<TDocument> collection)
        {
            return new CollectionExistsOperation(collection.DatabaseName, collection.CollectionName);
        }

        public static FindOperation<TDocument> Find<TDocument>(this Collection<TDocument> collection, BsonDocument query = null)
        {
            return new FindOperation<TDocument>(collection.DatabaseName, collection.CollectionName, collection.Serializer, query);
        }

        public static FindAndModifyOperation FindAndModify<TDocument>(this Collection<TDocument> collection, BsonDocument query, BsonDocument update, BsonDocument sort = null)
        {
            return new FindAndModifyOperation(collection.DatabaseName, collection.CollectionName, query, update, sort);
        }

        public static FindAndRemoveOperation FindAndRemove<TDocument>(this Collection<TDocument> collection, BsonDocument query, BsonDocument sort = null)
        {
            return new FindAndRemoveOperation(collection.DatabaseName, collection.CollectionName, query, sort);
        }

        public static FindOneOperation<TDocument> FindOne<TDocument>(this Collection<TDocument> collection, BsonDocument query = null)
        {
            return new FindOneOperation<TDocument>(collection.DatabaseName, collection.CollectionName, collection.Serializer, query);
        }

        public static GroupOperation Group<TDocument>(this Collection<TDocument> collection, BsonDocument key, BsonDocument initial, BsonJavaScript reduceFunction, BsonDocument query = null)
        {
            return new GroupOperation(collection.DatabaseName, collection.CollectionName, key, initial, reduceFunction, query);
        }

        public static GroupOperation Group<TDocument>(this Collection<TDocument> collection, BsonJavaScript keyFunction, BsonDocument initial, BsonJavaScript reduceFunction, BsonDocument query = null)
        {
            return new GroupOperation(collection.DatabaseName, collection.CollectionName, keyFunction, initial, reduceFunction, query);
        }

        public static IndexExistsOperation IndexExists<TDocument>(this Collection<TDocument> collection, BsonDocument keys)
        {
            return new IndexExistsOperation(collection.DatabaseName, collection.CollectionName, keys);
        }

        public static IndexExistsOperation IndexExists<TDocument>(this Collection<TDocument> collection, string indexName)
        {
            return new IndexExistsOperation(collection.DatabaseName, collection.CollectionName, indexName);
        }

        public static InsertCommandOperation<TDocument> Insert<TDocument>(this Collection<TDocument> collection, IEnumerable<TDocument> documents)
        {
            return new InsertCommandOperation<TDocument>(collection.DatabaseName, collection.CollectionName, collection.Serializer, documents);
        }

        public static InsertOpcodeOperation<TDocument> InsertUsingOpcode<TDocument>(this Collection<TDocument> collection, IEnumerable<TDocument> documents)
        {
            return new InsertOpcodeOperation<TDocument>(collection.DatabaseName, collection.CollectionName, collection.Serializer, documents);
        }

        public static MapReduceOperation MapReduce<TDocument>(this Collection<TDocument> collection, BsonJavaScript mapFunction, BsonJavaScript reduceFunction, BsonDocument query = null)
        {
            return new MapReduceOperation(collection.DatabaseName, collection.CollectionName, mapFunction, reduceFunction, query);
        }

        public static RenameCollectionOperation Rename<TDocument>(this Collection<TDocument> collection, string newCollectionName)
        {
            return new RenameCollectionOperation(collection.DatabaseName, collection.CollectionName, newCollectionName);
        }

        public static UpdateCommandOperation Update<TDocument>(this Collection<TDocument> collection, IEnumerable<UpdateRequest> updates)
        {
            return new UpdateCommandOperation(collection.DatabaseName, collection.CollectionName, updates);
        }

        public static UpdateOpcodeOperation UpdateUsingOpcode<TDocument>(this Collection<TDocument> collection, BsonDocument query, BsonDocument update)
        {
            return new UpdateOpcodeOperation(collection.DatabaseName, collection.CollectionName, query, update);
        }
    }
}
