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
    public class BsonUndefined : BsonValue
    {
        #region static
        // static fields
        private static readonly BsonUndefined __instance = new BsonUndefined();

        // static properties
        public static BsonUndefined Instance
        {
            get { return __instance; }
        }
        #endregion

        // constructors
        public BsonUndefined()
            : base(BsonType.Undefined)
        {
        }

        // methods
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BsonUndefined))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool ToBoolean()
        {
            return false;
        }

        public override string ToString()
        {
            return "undefined";
        }
    }
}
