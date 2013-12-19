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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public class Hasher
    {
        // fields
        private int _hashCode;

        // constructors
        public Hasher()
        {
            _hashCode = 17;
        }

        // methods
        public override int GetHashCode()
        {
            return _hashCode;
        }

        public Hasher Hash(bool value)
        {
            _hashCode = 37 * _hashCode + value.GetHashCode();
            return this;
        }

        public Hasher Hash(int value)
        {
            _hashCode = 37 * _hashCode + value.GetHashCode();
            return this;
        }

        public Hasher Hash(long value)
        {
            _hashCode = 37 * _hashCode + value.GetHashCode();
            return this;
        }

        public Hasher Hash(object value)
        {
            _hashCode = 37 * _hashCode + ((value == null) ? 0 : value.GetHashCode());
            return this;
        }

        public Hasher Hash(short value)
        {
            _hashCode = 37 * _hashCode + value.GetHashCode();
            return this;
        }

        public Hasher HashEnumerable(IEnumerable enumerable)
        {
            if (enumerable == null)
            {
                _hashCode = 37 * _hashCode + 0;
            }
            else
            {
                foreach (var obj in enumerable)
                {
                    _hashCode = 37 * _hashCode + ((obj == null) ? 0 : obj.GetHashCode());
                }
            }
            return this;
        }
    }
}
