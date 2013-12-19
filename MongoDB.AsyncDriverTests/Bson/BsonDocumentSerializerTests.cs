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
using MongoDB.AsyncDriver;
using NUnit.Framework;

namespace MongoDB.AsyncDriverTests
{
    [TestFixture]
    public class BsonDocumentSerializerTests
    {
        private BsonDocumentSerializer _serializer = new BsonDocumentSerializer();

        [Test]
        public void DeserializeEmptyDocument()
        {
            var stream = CreateStream("0500000000");
            var reader = new BsonReader(stream);
            var document = _serializer.Deserialize(reader);
            Assert.AreEqual(0, document.ElementCount);
        }

        [Test]
        public void DeserializeOneInt32()
        {
            var stream = CreateStream("0c0000001041000100000000");
            var reader = new BsonReader(stream);
            var document = _serializer.Deserialize(reader);
            Assert.AreEqual(1, document.ElementCount);
            var element = document.GetElement(0);
            Assert.AreEqual("A", element.Name);
            Assert.AreEqual(1, (int)element.Value);
        }

        [Test]
        public void SerializeOneInt32()
        {
            var document = new BsonDocument("A", 1);
            var stream = new MemoryStream();
            var primitiveWriter = new BsonPrimitiveWriter(stream);
            var bsonWriter = new BsonWriter(primitiveWriter);
            _serializer.Serialize(bsonWriter, document);
            Assert.AreEqual("0c0000001041000100000000", Hex.ToString(stream.ToArray()));
        }

        private Stream CreateStream(string hex)
        {
            var bytes = Hex.Parse(hex);
            return new MemoryStream(bytes);
        }
    }
}
