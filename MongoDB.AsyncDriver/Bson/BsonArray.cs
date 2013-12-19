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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public class BsonArray : BsonValue, IList<BsonValue>
    {
        // fields
        private readonly List<BsonValue> _items;

        // constructors
        public BsonArray()
            : base(BsonType.Array)
        {
            _items = new List<BsonValue>();
        }

        public BsonArray(IEnumerable<BsonValue> collection)
            : base(BsonType.Array)
        {
            _items = new List<BsonValue>(collection);
        }

        public BsonArray(int capacity)
            : base(BsonType.Array)
        {
            _items = new List<BsonValue>(capacity);
        }

        // properties
        public int Capacity
        {
            get { return _items.Capacity; }
            set { _items.Capacity = value; }
        }

        public int Count
        {
            get { return _items.Count; }
        }

        bool ICollection<BsonValue>.IsReadOnly
        {
            get { return false; }
        }

        // indexers
        public override BsonValue this[int index]
        {
            get
            {
                return _items[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _items[index] = value;
            }
        }

        // methods
        public void Add(BsonValue item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            _items.Add(item);
        }

        public void AddRange(IEnumerable<BsonValue> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (collection.Contains(null))
            {
                throw new ArgumentException("BsonValue cannot be null.");
            }
            _items.AddRange(collection);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(BsonValue item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            return _items.Contains(item);
        }

        public void CopyTo(BsonValue[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BsonArray))
            {
                return false;
            }

            var rhs = (BsonArray)obj;
            return _items.SequenceEqual(rhs._items);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<BsonValue> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return new Hasher()
                .HashEnumerable(_items)
                .GetHashCode();
        }

        public int IndexOf(BsonValue item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            return _items.IndexOf(item);
        }

        public void Insert(int index, BsonValue item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            _items.Insert(index, item);
        }

        public void InsertRange(int index, IEnumerable<BsonValue> collection)
        {
            _items.InsertRange(index, collection);
        }

        public bool Remove(BsonValue item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            return _items.Remove(item);
        }

        public int RemoveAll(Predicate<BsonValue> match)
        {
            return _items.RemoveAll(match);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        public BsonValue[] ToArray()
        {
            return _items.ToArray();
        }

        public List<BsonValue> ToList()
        {
            return _items.ToList();
        }

        public override string ToString()
        {
            if (_items.Count == 0)
            {
                return "[ ]";
            }
            else
            {
                return string.Format("[{0}]", string.Join(", ", _items.Select(i => i.ToString()).ToArray()));
            }
        }

        public void TrimExcess()
        {
            _items.TrimExcess();
        }
    }
}
