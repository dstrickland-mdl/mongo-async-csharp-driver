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
    /// <summary>
    /// Represents a read preference.
    /// </summary>
    public class ReadPreference
    {
        #region static
        // static fields
        private static readonly ReadPreference __nearest = new ReadPreference(ReadPreferenceMode.Nearest);
        private static readonly ReadPreference __primary = new ReadPreference(ReadPreferenceMode.Primary);
        private static readonly ReadPreference __primaryPreferred = new ReadPreference(ReadPreferenceMode.PrimaryPreferred);
        private static readonly ReadPreference __secondary = new ReadPreference(ReadPreferenceMode.Secondary);
        private static readonly ReadPreference __secondaryPreferred = new ReadPreference(ReadPreferenceMode.SecondaryPreferred);

        // static properties
        public static ReadPreference Nearest
        {
            get { return __nearest; }
        }

        public static ReadPreference Primary
        {
            get { return __primary; }
        }

        public static ReadPreference PrimaryPreferred
        {
            get { return __primaryPreferred; }
        }

        public static ReadPreference Secondary
        {
            get { return __secondary; }
        }

        public static ReadPreference SecondaryPreferred
        {
            get { return __secondaryPreferred; }
        }
        #endregion

        // fields
        private readonly ReadPreferenceMode _mode;
        private readonly IReadOnlyList<TagSet> _tagSets;

        // constructors
        public ReadPreference(ReadPreferenceMode mode)
        {
            _mode = mode;
        }

        public ReadPreference(ReadPreferenceMode mode, IEnumerable<TagSet> tagSets)
        {
            _mode = mode;
            _tagSets = tagSets.ToList();
        }

        // properties
        public ReadPreferenceMode Mode
        {
            get { return _mode; }
        }

        public IReadOnlyList<TagSet> TagSets
        {
            get { return _tagSets; }
        }

        // methods
        public ReadPreference WithMode(ReadPreferenceMode value)
        {
            return (_mode == value) ? this : new ReadPreference(value, _tagSets);
        }

        public ReadPreference WithTagSets(IEnumerable<TagSet> value)
        {
            return (object.Equals(_tagSets, value)) ? this : new ReadPreference(_mode, value);
        }
    }
}
