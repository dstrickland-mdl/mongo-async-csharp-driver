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
    public class BsonString : BsonValue
    {
        #region static
        // static operators
        public static implicit operator BsonString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return new BsonString(value);
        }
        #endregion

        // fields
        private readonly string _value;

        // constructors
        public BsonString(string value)
            : base(BsonType.String)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            _value = value;
        }

        // properties
        public string Value
        {
            get { return _value; }
        }

        // methods
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BsonString))
            {
                return false;
            }
            var rhs = (BsonString)obj;
            return _value == rhs.Value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override bool ToBoolean()
        {
            return _value != "";
        }

        public override double ToDouble()
        {
            return double.Parse(_value, CultureInfo.InvariantCulture);
        }

        public override int ToInt32()
        {
            return int.Parse(_value, CultureInfo.InvariantCulture);
        }

        public override long ToInt64()
        {
            return long.Parse(_value, CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return string.Format("'{0}'", _value.Replace("'", "\\'"));
        }
    }
}
