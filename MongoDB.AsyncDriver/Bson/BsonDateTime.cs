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
    public class BsonDateTime : BsonValue
    {
        #region static
        public static implicit operator BsonDateTime(DateTime value)
        {
            return new BsonDateTime(value);
        }
        #endregion

        // fields
        private readonly long _millisecondsSinceEpoch;

        // constructors
        public BsonDateTime(DateTime dateTime)
            : base(BsonType.DateTime)
        {
            _millisecondsSinceEpoch = Utils.ToMillisecondsSinceEpoch(dateTime);
        }

        public BsonDateTime(long millisecondsSinceEpoch)
            : base(BsonType.DateTime)
        {
            _millisecondsSinceEpoch = millisecondsSinceEpoch;
        }

        // properties
        public bool IsWithinDotNetRange
        {
            get
            {
                return
                    _millisecondsSinceEpoch >= Constants.DateTimeMinValueMillisecondsSinceEpoch &&
                    _millisecondsSinceEpoch <= Constants.DateTimeMaxValueMillisecondsSinceEpoch;
            }
        }

        public long MillisecondsSinceEpoch
        {
            get { return _millisecondsSinceEpoch; }
        }

        // methods
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BsonDateTime))
            {
                return false;
            }
            var rhs = (BsonDateTime)obj;
            return _millisecondsSinceEpoch == rhs._millisecondsSinceEpoch;
        }

        public override int GetHashCode()
        {
            return _millisecondsSinceEpoch.GetHashCode();
        }

        public override DateTime ToLocalTime()
        {
            return Utils.ToLocalTime(Utils.ToDateTimeFromMillisecondsSinceEpoch(_millisecondsSinceEpoch));
        }

        public override string ToString()
        {
            if (IsWithinDotNetRange)
            {
                var dateTime = Utils.ToDateTimeFromMillisecondsSinceEpoch(_millisecondsSinceEpoch);
                return string.Format("ISODate('{0}')", dateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFZ"));
            }
            else
            {
                return string.Format("new Date({0})", _millisecondsSinceEpoch.ToString(CultureInfo.InvariantCulture));
            }
        }

        public override DateTime ToUniversalTime()
        {
            return Utils.ToDateTimeFromMillisecondsSinceEpoch(_millisecondsSinceEpoch);
        }
    }
}
