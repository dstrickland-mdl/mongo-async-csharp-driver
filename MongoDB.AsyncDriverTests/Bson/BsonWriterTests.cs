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
    public class BsonWriterTests
    {
        private MemoryStream _memoryStream;
        private BsonWriter _writer;

        [SetUp]
        public void Setup()
        {
            _memoryStream = new MemoryStream();
            _writer = new BsonWriter(_memoryStream);
        }

        [Test]
        [TestCase(false, "00")]
        [TestCase(true, "01")]
        public void TestWriteBoolean(bool value, string expected)
        {
            _writer.WriteBoolean(value);
            Assert.AreEqual(expected, ToHexString());
        }

        [Test]
        public void TestWriteEndArray()
        {
            _writer.WriteStartArray();
            _writer.WriteEndArray();
            Assert.AreEqual("0500000000", ToHexString());
        }

        [Test]
        public void TestWriteEndDocument()
        {
            _writer.WriteStartDocument();
            _writer.WriteEndDocument();
            Assert.AreEqual("0500000000", ToHexString());
        }

        [Test]
        [TestCase("", "", "0000")]
        [TestCase("A", "", "410000")]
        [TestCase("", "B", "004200")]
        [TestCase("A", "B", "41004200")]
        public void TestWriteRegularExpression(string pattern, string options, string expected)
        {
            var value = new BsonRegularExpression(pattern, options);
            _writer.WriteRegularExpression(value);
            Assert.AreEqual(expected, ToHexString());
        }

        [Test]
        public void TestWriteStartArray()
        {
            _writer.WriteStartArray();
            Assert.AreEqual("00000000", ToHexString());
        }

        [Test]
        public void TestWriteStartDocument()
        {
            _writer.WriteStartDocument();
            Assert.AreEqual("00000000", ToHexString());
        }

        [Test]
        [TestCase("", "0100000000")]
        [TestCase("A", "020000004100")]
        [TestCase("AB", "03000000414200")]
        public void TestWriteString(string value, string expected)
        {
            _writer.WriteString(value);
            Assert.AreEqual(expected, ToHexString());
        }

        private string ToHexString()
        {
            return Hex.ToString(_memoryStream.ToArray());
        }
    }
}
