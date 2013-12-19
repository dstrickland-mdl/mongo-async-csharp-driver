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
    public class BsonReaderTests
    {
        [Test]
        [TestCase("00", false)]
        [TestCase("01", true)]
        [TestCase("02", true)]
        public void TestReadBoolean(string hex, bool expected)
        {
            var reader = Test.CreateReader(hex);
            Assert.AreEqual(expected, reader.ReadBoolean());
        }

        [Test]
        [TestCase("00000000", 0)]
        [TestCase("01000000", 1)]
        [TestCase("00010000", 256)]
        public void TestReadInt32(string hex, int expected)
        {
            var reader = Test.CreateReader(hex);
            Assert.AreEqual(expected, reader.ReadInt32());
        }

        [Test]
        [TestCase("0000000000000000", 0)]
        [TestCase("0100000000000000", 1)]
        [TestCase("0001000000000000", 256)]
        public void TestReadInt64(string hex, long expected)
        {
            var reader = Test.CreateReader(hex);
            Assert.AreEqual(expected, reader.ReadInt64());
        }

        [Test]
        [TestCase("0000", "", "")]
        [TestCase("410000", "A", "")]
        [TestCase("004200", "", "B")]
        [TestCase("41004200", "A", "B")]
        public void TestReadRegularExpression(string hex, string pattern, string options)
        {
            var reader = Test.CreateReader(hex);
            var value = reader.ReadRegularExpression();
            Assert.AreEqual(pattern, value.Pattern);
            Assert.AreEqual(options, value.Options);
        }
    }
}
