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
using MongoDB.AsyncDriver;
using NUnit.Framework;

namespace MongoDB.AsyncDriverTests
{
    [TestFixture]
    public class BsonInt32Tests
    {
        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        public void TestConstructor(int n)
        {
            var value = new BsonInt32(n);
            Assert.AreEqual(BsonType.Int32, value.Type);
            Assert.AreEqual(n, value.Value);
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        public void TestImplicitConversionToBsonInt32(int n)
        {
            var value = (BsonInt32)n;
            Assert.AreEqual(BsonType.Int32, value.Type);
            Assert.AreEqual(n, value.Value);
        }

        [Test]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void TestToBoolean(int n, bool expected)
        {
            Assert.AreEqual(expected, new BsonInt32(n).ToBoolean());
        }
    }
}
