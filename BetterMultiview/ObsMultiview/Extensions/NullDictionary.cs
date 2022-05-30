using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObsMultiview.Extensions {
    internal class NullDictionary<T, U> : IDictionary<T, U> {
        private Dictionary<T,U> _dictionary = new Dictionary<T,U>();

        public IEnumerator<KeyValuePair<T, U>> GetEnumerator() =>_dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>_dictionary.GetEnumerator();

        public void Add(KeyValuePair<T, U> item) => _dictionary.Add(item.Key, item.Value);

        public void Clear() => _dictionary.Clear();

        public bool Contains(KeyValuePair<T, U> item) => _dictionary.Contains(item);
        public void CopyTo(KeyValuePair<T, U>[] array, int arrayIndex) =>((IDictionary<T,U>)_dictionary).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<T, U> item) => ((IDictionary<T,U>)_dictionary).Remove(item);

        public int Count =>_dictionary.Count;
        public bool IsReadOnly=> ((IDictionary<T,U>)_dictionary).IsReadOnly;
        public void Add(T key, U value) => _dictionary.Add(key, value);

        public bool ContainsKey(T key) => _dictionary.ContainsKey(key);

        public bool Remove(T key) => _dictionary.Remove(key);

        public bool TryGetValue(T key, out U value)=> _dictionary.TryGetValue(key, out value);

        public U this[T key] {
            get => _dictionary.ContainsKey(key) ? _dictionary[key] : default(U);
            set => _dictionary[key] = value;
        }

        public ICollection<T> Keys => _dictionary.Keys;
        public ICollection<U> Values => _dictionary.Values;
    }
}