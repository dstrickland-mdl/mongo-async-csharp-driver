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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public class BsonInt64 : BsonValue
    {
        #region static
        // static operators
        public static implicit operator BsonInt64(long value)
        {
            return new BsonInt64(value);
        }
        #endregion

        // fields
        private readonly long _value;

        // constructors
        public BsonInt64(long value)
            : base(BsonType.Int64)
        {
            _value = value;
        }

        // properties
        public long Value
        {
            get { return _value; }
        }

        // methods
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BsonInt64))
            {
                return false;
            }
            var rhs = (BsonInt64)obj;
            return _value == rhs._value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override bool ToBoolean()
        {
            return _value != 0;
        }

        public override double ToDouble()
        {
            return (double)_value;
        }

        public override int ToInt32()
        {
            return (int)_value;
        }

        public override long ToInt64()
        {
            return (long)_value;
        }

        public override string ToString()
        {
            return string.Format("NumberLong({0})", _value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
