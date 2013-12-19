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
    public static class DatabaseExtensionMethods
    {
        // methods
        public static DropDatabaseOperation Drop(this Database database)
        {
            return new DropDatabaseOperation(database.DatabaseName);
        }

        public static EvalOperation Eval(this Database database, BsonJavaScript javaScript)
        {
            return new EvalOperation(database.DatabaseName, javaScript);
        }

        public static DatabaseExistsOperation Exists(this Database database)
        {
            return new DatabaseExistsOperation(database.DatabaseName);
        }

        public static ListCollectionsOperation ListCollections<TDocument>(this Database database)
        {
            return new ListCollectionsOperation(database.DatabaseName);
        }

        public static ReadCommandOperation ReadCommand(this Database database, BsonDocument command)
        {
            return new ReadCommandOperation(database.DatabaseName, command);
        }

        public static WriteCommandOperation WriteCommand(this Database database, BsonDocument command)
        {
            return new WriteCommandOperation(database.DatabaseName, command);
        }
    }
}
