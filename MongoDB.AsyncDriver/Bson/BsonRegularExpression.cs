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
    public class BsonRegularExpression : BsonValue
    {
        #region static
        // static operators
        public static implicit operator BsonRegularExpression(string value)
        {
            return new BsonRegularExpression(value);
        }
        #endregion

        // fields
        private readonly string _pattern;
        private readonly string _options;

        // constructors
        public BsonRegularExpression(string pattern)
            : base(BsonType.RegularExpression)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }
            if (pattern.Length > 0 && pattern[0] == '/')
            {
                var index = pattern.LastIndexOf('/');
                var escaped = pattern.Substring(1, index - 1);
                var unescaped = (escaped == "(?:)") ? "" : escaped.Replace("\\/", "/");
                _pattern = unescaped;
                _options = pattern.Substring(index + 1);
            }
            else
            {
                _pattern = pattern;
                _options = "";
            }
        }

        public BsonRegularExpression(string pattern, string options)
            : base(BsonType.RegularExpression)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            _pattern = pattern;
            _options = options;
        }

        // properties
        public string Pattern
        {
            get { return _pattern; }
        }

        public string Options
        {
            get { return _options; }
        }

        // methods
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BsonRegularExpression))
            {
                return false;
            }
            var rhs = (BsonRegularExpression)obj;
            return _pattern == rhs._pattern && _options == rhs._options;
        }

        public override int GetHashCode()
        {
            return new Hasher()
                .Hash(_pattern)
                .Hash(_options)
                .GetHashCode();
        }

        public override string ToString()
        {
            var escaped = (_pattern == "") ? "(?:)" : _pattern.Replace("/", @"\/");
            return string.Format("/{0}/{1}", escaped, _options);
        }
    }
}
