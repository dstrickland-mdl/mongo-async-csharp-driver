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
    public abstract class BsonValue
    {
        #region static
        // static operators
        public static implicit operator BsonValue(bool value)
        {
            return new BsonBoolean(value);
        }

        public static implicit operator BsonValue(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return new BsonBinaryData(value);
        }

        public static implicit operator BsonValue(DateTime value)
        {
            return new BsonDateTime(value);
        }

        public static implicit operator BsonValue(double value)
        {
            return new BsonDouble(value);
        }

        public static implicit operator BsonValue(Guid value)
        {
            return new BsonBinaryData(value);
        }

        public static implicit operator BsonValue(int value)
        {
            return new BsonInt32(value);
        }

        public static implicit operator BsonValue(long value)
        {
            return new BsonInt64(value);
        }

        public static implicit operator BsonValue(ObjectId value)
        {
            return new BsonObjectId(value);
        }

        public static implicit operator BsonValue(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return new BsonString(value);
        }

        public static explicit operator bool(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return ((BsonBoolean)value).Value;
        }

        public static explicit operator byte[](BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            var binaryData = (BsonBinaryData)value;
            var subType = binaryData.SubType;
            if (subType != BinarySubType.Binary)
            {
                throw new InvalidCastException("BsonBinaryData SubType is not Binary.");
            }
            return binaryData.Bytes;
        }

        public static explicit operator DateTime(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return ((BsonDateTime)value).ToUniversalTime();
        }

        public static explicit operator Guid(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return ((BsonBinaryData)value).ToGuid();
        }

        public static explicit operator double(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return ((BsonDouble)value).Value;
        }

        public static explicit operator int(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return ((BsonInt32)value).Value;
        }

        public static explicit operator long(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return ((BsonInt64)value).Value;
        }

        public static explicit operator ObjectId(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return ((BsonObjectId)value).Value;
        }

        public static explicit operator string(BsonValue value)
        {
            return (value != null) ? ((BsonString)value).Value : null;
        }
        #endregion

        // fields
        private readonly BsonType _type;

        // constructors
        protected BsonValue(BsonType type)
        {
            _type = type;   
        }

        // properties
        public BsonType Type
        {
            get { return _type; }
        }

        // indexers
        public virtual BsonValue this[int index]
        {
            get
            {
                var message = string.Format("{0} does not support indexing by position (only BsonDocument and BsonArray do).", this.GetType().Name);
                throw new NotSupportedException(message);
            }
            set
            {
                var message = string.Format("{0} does not support indexing by position (only BsonDocument and BsonArray do).", this.GetType().Name);
                throw new NotSupportedException(message);
            }
        }

        public virtual BsonValue this[string name]
        {
            get
            {
                var message = string.Format("{0} does not support indexing by name (only BsonDocument does).", this.GetType().Name);
                throw new NotSupportedException(message);
            }
            set
            {
                var message = string.Format("{0} does not support indexing by name (only BsonDocument does).", this.GetType().Name);
                throw new NotSupportedException(message);
            }
        }

        // methods
        public override bool Equals(object obj)
        {
            throw new NotImplementedException("Subclasses must override Equals.");
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException("Subclasses must override GetHashCode.");
        }

        public virtual bool ToBoolean()
        {
            return true;
        }

        public virtual double ToDouble()
        {
            throw new InvalidCastException();
        }

        public virtual int ToInt32()
        {
            throw new InvalidCastException();
        }

        public virtual long ToInt64()
        {
            throw new InvalidCastException();
        }

        public virtual DateTime ToLocalTime()
        {
            throw new InvalidCastException();
        }

        public virtual DateTime ToUniversalTime()
        {
            throw new InvalidCastException();
        }
    }
}
