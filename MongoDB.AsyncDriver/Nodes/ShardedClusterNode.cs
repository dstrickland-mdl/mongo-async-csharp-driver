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
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    /// <summary>
    /// Represents a sharded cluster node that was selected using a ReadPreference (it remembers the ReadPrefence that should be sent to mongos).
    /// </summary>
    internal abstract class ShardedClusterNode : WrappedNode
    {
        // fields
        private readonly ReadPreference _readPreference;

        // constructors
        public ShardedClusterNode(INode wrapped, ReadPreference readPreference)
            : base(wrapped)
        {
            _readPreference = readPreference;
        }

        // properties
        public ReadPreference ReadPreference
        {
            get { return _readPreference; }
        }

        // methods
        public BsonDocument CreateReadPreferenceDocument()
        {
            if (_readPreference != ReadPreference.Primary)
            {
                BsonArray tagSets = null;
                if (_readPreference.TagSets != null)
                {
                    tagSets = new BsonArray(_readPreference.TagSets.Select(ts => new BsonDocument(ts.Tags.Select(t => new BsonElement(t.Name, t.Value)))));
                }

                if (_readPreference != ReadPreference.SecondaryPreferred || tagSets != null)
                {
                    return new BsonDocument
                    {
                        { "mode", Utils.ToCamelCase(_readPreference.Mode.ToString()) },
                        { "tags", tagSets, tagSets != null }
                    };
                }
            }

            return null;
        }
    }
}
