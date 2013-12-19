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
    public class BsonJavaScript : BsonValue
    {
        #region static
        // static operators
        public static implicit operator BsonJavaScript(string code)
        {
            return new BsonJavaScript(code);
        }
        #endregion

        // fields
        private readonly string _code;
        private readonly BsonDocument _scope;

        // constructors
        public BsonJavaScript(string code)
            : base(BsonType.JavaScript)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }
            _code = code;
        }

        public BsonJavaScript(string code, BsonDocument scope)
            : base((scope == null) ? BsonType.JavaScript : BsonType.JavaScriptWithScope)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }
            _code = code;
            _scope = scope;
        }

        // properties
        public string Code
        {
            get { return _code; }
        }

        public BsonDocument Scope
        {
            get { return _scope; }
        }
        
        // methods
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BsonJavaScript))
            {
                return false;
            }
            var rhs = (BsonJavaScript)obj;
            return
                _code == rhs._code &&
                object.Equals(_scope, rhs._scope);
        }

        public override int GetHashCode()
        {
            return new Hasher()
                .Hash(_code)
                .Hash(_scope)
                .GetHashCode();
        }

        public override string ToString()
        {
            if (_scope == null)
            {
                return string.Format("JavaScript('{0}')", _code.Replace("'", "\\'"));
            }
            else
            {
                return string.Format("JavaScript('{0}', {1})", _code.Replace("'", "\\'"), _scope);
            }
        }
    }
}
