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
    public class BsonElement
    {
        // fields
        private readonly string _name;
        private readonly BsonValue _value;

        // constructors
        public BsonElement(string name, BsonValue value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            _name = name;
            _value = value;
        }

        // properties
        public string Name
        {
            get { return _name; }
        }

        public BsonValue Value
        {
            get { return _value; }
        }

        // methods
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BsonElement))
            {
                return false;
            }
            var rhs = (BsonElement)obj;
            return
                _name == rhs._name &&
                _value.Equals(rhs._value);
        }

        public override int GetHashCode()
        {
            return new Hasher()
                .Hash(_name)
                .Hash(_value)
                .GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("'{0}' : {1}", _name.Replace("'", "\\'"), _value);
        }
    }
}
