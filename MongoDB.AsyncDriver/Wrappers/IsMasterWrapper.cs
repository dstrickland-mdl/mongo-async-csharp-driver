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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public class IsMasterWrapper
    {
        // fields
        private readonly BsonDocument _document;

        // constructors
        public IsMasterWrapper(BsonDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            _document = document;
        }

        // properties
        public BsonDocument Document
        {
            get { return _document; }
        }

        public bool IsReplicaSetMember
        {
            get
            {
                return _document.GetValue("isreplicaset", false).ToBoolean() || _document.ContainsName("setName");
            }
        }

        public int MaxDocumentSize
        {
            get
            {
                BsonValue value;
                if (_document.TryGetValue("maxBsonObjectSize", out value))
                {
                    return value.ToInt32();
                }
                else
                {
                    return 4 * 1024 * 1024;
                }
            }
        }

        public int MaxMessageSize
        {
            get
            {
                BsonValue value;
                if (_document.TryGetValue("maxMessageSizeBytes", out value))
                {
                    return value.ToInt32();
                }
                else
                {
                    return Math.Max(MaxDocumentSize + 1024, 16000000);
                }
            }
        }

        public int MaxWireDocumentSize
        {
            get
            {
                BsonValue value;
                if (_document.TryGetValue("maxBsonWireObjectSize", out value))
                {
                    return value.ToInt32();
                }
                else
                {
                    return MaxDocumentSize + 16 * 1024;
                }
            }
        }

        public NodeType NodeType
        {
            get
            {
                if (IsReplicaSetMember)
                {
                    if (_document.GetValue("ismaster", false).ToBoolean())
                    {
                        return NodeType.Primary;
                    }
                    if (_document.GetValue("secondary", false).ToBoolean())
                    {
                        return NodeType.Secondary;
                    }
                    if (_document.GetValue("passive", false).ToBoolean() || _document.GetValue("hidden", false).ToBoolean())
                    {
                        return NodeType.Passive;
                    }
                    if (_document.GetValue("arbiterOnly", false).ToBoolean())
                    {
                        return NodeType.Arbiter;
                    }
                    return NodeType.Other;
                }

                if ((string)_document.GetValue("msg", null) == "isdbgrid")
                {
                    return NodeType.ShardRouter;
                }

                if (_document.GetValue("ismaster", false).ToBoolean())
                {
                    return NodeType.StandAlone;
                }

                return NodeType.Unknown;
            }
        }

        public TagSet Tags
        {
            get
            {
                BsonValue tags;
                if (_document.TryGetValue("tags", out tags))
                {
                    return new TagSet(tags.AsBsonDocument().Select(e => new Tag(e.Name, (string)e.Value)));
                }
                else
                {
                    return null;
                }
            }
        }

        // methods
        private List<DnsEndPoint> GetMembers(AddressFamily addressFamily)
        {
            var hosts = GetMembers(addressFamily, "hosts");
            var passives = GetMembers(addressFamily, "passives");
            var arbiters = GetMembers(addressFamily, "arbiters");
            return hosts.Concat(passives).Concat(arbiters).ToList();
        }

        private IEnumerable<DnsEndPoint> GetMembers(AddressFamily addressFamily, string name)
        {
            if (!_document.ContainsName(name))
            {
                return new DnsEndPoint[0];
            }

            return ((BsonArray)_document[name]).Select(v => DnsEndPointParser.Parse((string)v, addressFamily));
        }

        private DnsEndPoint GetPrimary(AddressFamily addressFamily)
        {
            BsonValue primary;
            if (_document.TryGetValue("primary", out primary))
            {
                // TODO: what does primary look like when there is no current primary (null, empty string)?
                return DnsEndPointParser.Parse((string)primary, addressFamily);
            }

            return null;
        }

        public ReplicaSetConfig GetReplicaSetConfig(AddressFamily addressFamily)
        {
            if (!IsReplicaSetMember)
            {
                return null;
            }

            var members = GetMembers(addressFamily);
            var name = (string)_document.GetValue("setName", null);
            var primary = GetPrimary(addressFamily);
            var version = _document.ContainsName("version") ? (int?)_document["version"].ToInt32() : null;

            return new ReplicaSetConfig(members, name, primary, version);
        }
    }
}
