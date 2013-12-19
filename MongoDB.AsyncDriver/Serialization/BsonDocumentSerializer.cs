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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public class BsonDocumentSerializer : IBsonSerializer<BsonDocument>
    {
        #region static
        // static fields
        private static readonly BsonDocumentSerializer __instance = new BsonDocumentSerializer();

        // static properties
        public static BsonDocumentSerializer Instance
        {
            get { return __instance; }
        }
        #endregion

        // methods
        public BsonDocument Deserialize(IBsonReader reader)
        {
            return DeserializeDocument(reader);
        }

        private BsonArray DeserializeArray(IBsonReader reader)
        {
            var array = new BsonArray();

            reader.ReadStartArray();
            BsonType type;
            while ((type = reader.ReadType()) != 0)
            {
                var value = DeserializeValue(reader, type);
                array.Add(value);
            }
            reader.ReadEndArray();

            return array;
        }

        private BsonDocument DeserializeDocument(IBsonReader reader)
        {
            var document = new BsonDocument();

            reader.ReadStartDocument();
            BsonType type;
            while ((type = reader.ReadType()) != 0)
            {
                var name = reader.ReadName();
                var value = DeserializeValue(reader, type);
                document.Add(name, value);
            }
            reader.ReadEndDocument();

            return document;
        }

        private BsonValue DeserializeValue(IBsonReader reader, BsonType type)
        {
            switch (type)
            {
                case BsonType.Array: return DeserializeArray(reader);
                case BsonType.Binary: return reader.ReadBinaryData();
                case BsonType.Boolean: return new BsonBoolean(reader.ReadBoolean());
                case BsonType.DateTime: return reader.ReadDateTime();
                case BsonType.Document: return DeserializeDocument(reader);
                case BsonType.Double: return new BsonDouble(reader.ReadDouble());
                case BsonType.Int32: return new BsonInt32(reader.ReadInt32());
                case BsonType.Int64: return new BsonInt64(reader.ReadInt64());
                case BsonType.JavaScript:
                case BsonType.JavaScriptWithScope: return reader.ReadJavaScript(type);
                case BsonType.MaxKey: return BsonMaxKey.Instance;
                case BsonType.MinKey: return BsonMinKey.Instance;
                case BsonType.Null: return BsonNull.Instance;
                case BsonType.ObjectId: return new BsonObjectId(reader.ReadObjectId());
                case BsonType.RegularExpression: return reader.ReadRegularExpression();
                case BsonType.String: return reader.ReadString();
                case BsonType.Symbol: return reader.ReadSymbol();
                case BsonType.Timestamp: return reader.ReadTimestamp();
                case BsonType.Undefined: return BsonUndefined.Instance;
                default: throw new ArgumentException("Invalid BsonType.");
            }
        }

        public void Serialize(IBsonWriter writer, BsonDocument document)
        {
            SerializeDocument(writer, document);
        }

        private void SerializeArray(IBsonWriter writer, BsonArray array)
        {
            writer.WriteStartArray();
            foreach (var value in array)
            {
                SerializeValue(writer, value);
            }
            writer.WriteEndArray();
        }

        private void SerializeDocument(IBsonWriter writer, BsonDocument document)
        {
            writer.WriteStartDocument();
            foreach (var element in document)
            {
                writer.WriteName(element.Name);
                SerializeValue(writer, element.Value);
            }
            writer.WriteEndDocument();
       }

        private void SerializeValue(IBsonWriter writer, BsonValue value)
        {
            switch (value.Type)
            {
                case BsonType.Array: SerializeArray(writer, (BsonArray)value); break;
                case BsonType.Binary: writer.WriteBinaryData((BsonBinaryData)value); break;
                case BsonType.Boolean: writer.WriteBoolean(((BsonBoolean)value).Value); break;
                case BsonType.DateTime: writer.WriteDateTime((BsonDateTime)value); break;
                case BsonType.Document: SerializeDocument(writer, (BsonDocument)value); break;
                case BsonType.Double: writer.WriteDouble(((BsonDouble)value).Value); break;
                case BsonType.Int32: writer.WriteInt32(((BsonInt32)value).Value); break;
                case BsonType.Int64: writer.WriteInt64(((BsonInt64)value).Value); break;
                case BsonType.JavaScript: case BsonType.JavaScriptWithScope: writer.WriteJavaScript((BsonJavaScript)value); break;
                case BsonType.MaxKey: break;
                case BsonType.MinKey: break;
                case BsonType.Null: break;
                case BsonType.ObjectId: writer.WriteObjectId(((BsonObjectId)value).Value); break;
                case BsonType.RegularExpression: writer.WriteRegularExpression((BsonRegularExpression)value); break;
                case BsonType.String: writer.WriteString(((BsonString)value).Value); break;
                case BsonType.Symbol: writer.WriteSymbol((BsonSymbol)value); break;
                case BsonType.Timestamp: writer.WriteTimestamp((BsonTimestamp)value); break;
                case BsonType.Undefined: break;
                default: throw new ArgumentException("Invalid BsonType.");
            }
        }
    }
}
