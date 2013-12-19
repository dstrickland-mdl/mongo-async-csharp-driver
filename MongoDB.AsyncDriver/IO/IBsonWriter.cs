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
    public interface IBsonWriter
    {
        // methods
        void WriteBinaryData(BsonBinaryData value);
        void WriteBoolean(bool value);
        void WriteDateTime(BsonDateTime value);
        void WriteDouble(double value);
        void WriteEndArray();
        void WriteEndDocument();
        void WriteInt32(int value);
        void WriteInt64(long value);
        void WriteJavaScript(BsonJavaScript value);
        void WriteMaxKey();
        void WriteMinKey();
        void WriteName(string name);
        void WriteNull();
        void WriteObjectId(ObjectId value);
        void WriteRegularExpression(BsonRegularExpression value);
        void WriteStartArray();
        void WriteStartDocument();
        void WriteString(string value);
        void WriteSymbol(BsonSymbol value);
        void WriteTimestamp(BsonTimestamp value);
        void WriteUndefined();
    }
}
