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
    public class BsonBinaryData : BsonValue
    {
        #region static
        // static operators
        public static implicit operator BsonBinaryData(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return new BsonBinaryData(value);
        }

        public static implicit operator BsonBinaryData(Guid value)
        {
            return new BsonBinaryData(value);
        }
        #endregion

        // fields
        private readonly byte[] _bytes;
        private readonly BinarySubType _subType;

        // constructors
        public BsonBinaryData(byte[] bytes)
            : this(bytes, BinarySubType.Binary)
        {
        }

        public BsonBinaryData(byte[] bytes, BinarySubType subType)
            : base(BsonType.Binary)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            _bytes = bytes;
            _subType = subType;
        }

        public BsonBinaryData(Guid guid)
            : this(guid, GuidRepresentation.Standard)
        {
        }

        public BsonBinaryData(Guid guid, GuidRepresentation representation)
            : base(BsonType.Binary)
        {
            _bytes = GuidConverter.ToBytes(guid, representation);
            _subType = (representation == GuidRepresentation.Standard) ? BinarySubType.Uuid : BinarySubType.UuidLegacy;
        }

        // properties
        public byte[] Bytes
        {
            get { return _bytes; }
        }

        public BinarySubType SubType
        {
            get { return _subType; }
        }

        // methods
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BsonBinaryData))
            {
                return false;
            }

            var rhs = (BsonBinaryData)obj;
            return
                _subType == rhs.SubType &&
                _bytes.SequenceEqual(rhs._bytes);
        }

        public override int GetHashCode()
        {
            return new Hasher()
                .Hash(_subType)
                .Hash(_bytes)
                .GetHashCode();
        }

        public Guid ToGuid()
        {
            if (_subType == BinarySubType.Uuid)
            {
                return GuidConverter.FromBytes(_bytes, GuidRepresentation.Standard);
            }
            if (_subType == BinarySubType.UuidLegacy)
            {
                throw new NotSupportedException("You must specify the representation for sub type UuidLegacy.");
            }
            throw new NotSupportedException("The BSON binary data value is not a UUID.");
        }

        public Guid ToGuid(GuidRepresentation representation)
        {
            if (_subType == BinarySubType.Uuid)
            {
                return GuidConverter.FromBytes(_bytes, GuidRepresentation.Standard);
            }
            if (_subType == BinarySubType.UuidLegacy)
            {
                return GuidConverter.FromBytes(_bytes, representation);
            }
            throw new NotSupportedException("The BSON binary data value is not a UUID.");
        }

        public override string ToString()
        {
            return string.Format("HexData({0}, '{1}')", (int)_subType, Hex.ToString(_bytes));
        }
    }
}
