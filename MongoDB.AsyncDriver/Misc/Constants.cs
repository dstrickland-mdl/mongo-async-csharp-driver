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
    public static class Constants
    {
        // static fields
        private static readonly long __dateTimeMaxValueMillisecondsSinceEpoch;
        private static readonly long __dateTimeMinValueMillisecondsSinceEpoch;
        private static readonly DateTime __unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // static constructor
        static Constants()
        {
            // unixEpoch has to be initialized first
            __dateTimeMaxValueMillisecondsSinceEpoch = (DateTime.MaxValue - __unixEpoch).Ticks / 10000;
            __dateTimeMinValueMillisecondsSinceEpoch = (DateTime.MinValue - __unixEpoch).Ticks / 10000;
        }

        // static properties
        public static long DateTimeMaxValueMillisecondsSinceEpoch
        {
            get { return __dateTimeMaxValueMillisecondsSinceEpoch; }
        }

        public static long DateTimeMinValueMillisecondsSinceEpoch
        {
            get { return __dateTimeMinValueMillisecondsSinceEpoch; }
        }

        public static DateTime UnixEpoch
        {
            get { return __unixEpoch; }
        }
    }
}
