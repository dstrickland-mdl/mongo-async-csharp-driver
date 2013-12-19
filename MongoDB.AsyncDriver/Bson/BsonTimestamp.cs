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
    public class BsonTimestamp : BsonValue
    {
        // fields
        private readonly int _timestamp;
        private readonly int _increment;

        // constructors
        public BsonTimestamp(int timestamp, int increment)
            : base(BsonType.Timestamp)
        {
            _timestamp = timestamp;
            _increment = increment;
        }

        public BsonTimestamp(long value)
            : base(BsonType.Timestamp)
        {
            _timestamp = (int)(value >> 32);
            _increment = (int)value;
        }

        // properties
        public int Increment
        {
            get { return _increment; }
        }

        public int Timestamp
        {
            get { return _timestamp; }
        }

        // methods
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BsonTimestamp))
            {
                return false;
            }
            var rhs = (BsonTimestamp)obj;
            return _timestamp == rhs._timestamp && _increment == rhs._increment;
        }

        public override int GetHashCode()
        {
            return new Hasher()
                .Hash(_timestamp)
                .Hash(_increment)
                .GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Timestamp({0}, {1})", _timestamp, _increment);
        }
    }
}
