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
    public class BsonDocument : BsonValue, IEnumerable<BsonElement>
    {
        // fields
        private readonly List<BsonElement> _elements;
        private readonly Dictionary<string, int> _indexes;

        // constructors
        public BsonDocument()
            : base(BsonType.Document)
        {
            _elements = new List<BsonElement>();
            _indexes = new Dictionary<string, int>();
        }

        public BsonDocument(IEnumerable<BsonElement> elements)
            : this()
        {
            AddRange(elements);
        }

        public BsonDocument(int capacity)
            : base(BsonType.Document)
        {
            _elements = new List<BsonElement>(capacity);
            _indexes = new Dictionary<string, int>(capacity);
        }

        public BsonDocument(string name, BsonValue value)
            : this()
        {
            Add(name, value);
        }

        // properties
        public int ElementCount
        {
            get { return _elements.Count; }
        }

        public IEnumerable<BsonElement> Elements
        {
            get { return _elements; }
        }

        public IEnumerable<string> Names
        {
            get { return _elements.Select(e => e.Name); }
        }

        public IEnumerable<BsonValue> Values
        {
            get { return _elements.Select(e => e.Value); }
        }

        // indexers
        public override BsonValue this[int index]
        {
            get
            {
                return _elements[index].Value;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _elements[index] = new BsonElement(_elements[index].Name, value);
            }
        }

        public override BsonValue this[string name]
        {
            get
            {
                return _elements[_indexes[name]].Value;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                var element = new BsonElement(name, value);
                int index;
                if (_indexes.TryGetValue(name, out index))
                {
                    _elements[index] = element;
                }
                else
                {
                    _indexes.Add(name, _elements.Count);
                    _elements.Add(element);
                }
            }
        }

        // methods
        public void Add(BsonElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            _indexes.Add(element.Name, _elements.Count);
            _elements.Add(element);
        }

        public void Add(string name, BsonValue value)
        {
            var element = new BsonElement(name, value);
            _indexes.Add(element.Name, _elements.Count);
            _elements.Add(element);
        }

        public void Add(string name, BsonValue value, bool condition)
        {
            if (condition)
            {
                Add(name, value);
            }
        }

        public void Add(string name, Func<BsonValue> valueFactory, bool condition)
        {
            if (condition)
            {
                Add(name, valueFactory());
            }
        }

        public void AddRange(IEnumerable<BsonElement> elements)
        {
            elements.ForEach(e => Add(e));
        }

        public void AddRange(IEnumerable<KeyValuePair<string, BsonValue>> pairs)
        {
            pairs.ForEach(p => Add(p.Key, p.Value));
        }

        public void Clear()
        {
            _elements.Clear();
            _indexes.Clear();
        }

        public bool ContainsName(string name)
        {
            return _indexes.ContainsKey(name);
        }

        public bool ContainsValue(BsonValue value)
        {
            return _elements.Any(e => e.Value == value);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BsonDocument))
            {
                return false;
            }
            var rhs = (BsonDocument)obj;
            return _elements.SequenceEqual(rhs._elements);
        }

        public BsonElement GetElement(int index)
        {
            return _elements[index];
        }

        public BsonElement GetElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            var index = _indexes[name];
            return _elements[index];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<BsonElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return new Hasher()
                .HashEnumerable(_elements)
                .GetHashCode();
        }

        public BsonValue GetValue(int index)
        {
            return _elements[index].Value;
        }

        public BsonValue GetValue(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            var index = _indexes[name];
            return _elements[index].Value;
        }

        public BsonValue GetValue(string name, BsonValue defaultValue)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            int index;
            if (_indexes.TryGetValue(name, out index))
            {
                return _elements[index].Value;
            }
            else
            {
                return defaultValue;
            }
        }

        public void InsertAt(int index, BsonElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (_indexes.ContainsKey(element.Name))
            {
                throw new ArgumentException("An element with the same name already exists in the document.");
            }
            _elements.Insert(index, element);
            RebuildIndexes();
        }

        public void InsertAt(int index, string name, BsonValue value)
        {
            InsertAt(index, new BsonElement(name, value));
        }

        private void RebuildIndexes()
        {
            _indexes.Clear();
            _elements.ForEach((e, i) => _indexes[e.Name] = i);
        }

        public void Remove(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            int index;
            if (_indexes.TryGetValue(name, out index))
            {
                _elements.RemoveAt(index);
                RebuildIndexes();
            }
        }

        public void RemoveAt(int index)
        {
            _elements.RemoveAt(index);
            RebuildIndexes();
        }

        public void RemoveElement(BsonElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (_elements.Remove(element))
            {
                RebuildIndexes();
            }
        }

        public override string ToString()
        {
            if (_elements.Count == 0)
            {
                return "{ }";
            }
            else
            {
                return string.Format("{{ {0} }}", string.Join(", ", _elements.Select(e => e.ToString()).ToArray()));
            }
        }

        public bool TryGetElement(string name, out BsonElement element)
        {
            int index;
            if (_indexes.TryGetValue(name, out index))
            {
                element = _elements[index];
                return true;
            }
            else
            {
                element = null;
                return false;
            }
        }

        public bool TryGetValue(string name, out BsonValue value)
        {
            int index;
            if (_indexes.TryGetValue(name, out index))
            {
                value = _elements[index].Value;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}
