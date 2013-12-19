﻿/* Copyright 2013-2014 MongoDB Inc.
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
    public class BsonBoolean : BsonValue
    {
        #region static
        // static operators
        public static implicit operator BsonBoolean(bool value)
        {
            return new BsonBoolean(value);
        }
        #endregion

        // fields
        public readonly bool _value;

        // constructors
        public BsonBoolean(bool value)
            : base(BsonType.Boolean)
        {
            _value = value;
        }

        // properties
        public bool Value
        {
            get { return _value; }
        }

        // methods
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BsonBoolean))
            {
                return false;
            }
            var rhs = (BsonBoolean)obj;
            return _value == rhs._value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override bool ToBoolean()
        {
            return _value;
        }

        public override string ToString()
        {
            return _value ? "true" : "false";
        }
    }
}
