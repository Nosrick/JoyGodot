using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JoyLib.Code.Collections
{
    public class BucketCollection<K, T> : IEnumerable<KeyValuePair<K, List<T>>>
    {
        protected List<KeyValuePair<K, List<T>>> m_KeyValues;

        public BucketCollection()
        {
            this.m_KeyValues = new List<KeyValuePair<K, List<T>>>();
        }

        public BucketCollection(int capacity)
        {
            this.m_KeyValues = new List<KeyValuePair<K, List<T>>>(capacity);
        }

        public BucketCollection(BucketCollection<K, T> collection)
        {
            this.m_KeyValues = new List<KeyValuePair<K, List<T>>>(collection.m_KeyValues);
        }

        public bool Add(K key, T value)
        {
            if (this.m_KeyValues.Any(x => x.Key.Equals(key)))
            {
                this.m_KeyValues.First(x => x.Key.Equals(key)).Value.Add(value);
                return true;
            }

            List<T> newList = new List<T>();
            newList.Add(value);
            this.m_KeyValues.Add(new KeyValuePair<K, List<T>>(key, newList));
            return true;
        }

        public bool AddRange(K key, IEnumerable<T> collection)
        {
            if (this.m_KeyValues.Any(x => x.Key.Equals(key)))
            {
                this.m_KeyValues.First(x => x.Key.Equals(key)).Value.AddRange(collection);
                return true;
            }

            this.m_KeyValues.Add(new KeyValuePair<K, List<T>>(key, new List<T>(collection)));
            return true;
        }

        public bool Remove(K key)
        {
            try
            {
                this.m_KeyValues.Remove(this.m_KeyValues.First(x => x.Key.Equals(key)));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int RemoveForValue(T value)
        {
            Dictionary<K, List<T>> removals = this.m_KeyValues.Where(x => x.Value.Equals(value))
                                                .ToDictionary(x => x.Key, x => x.Value);

            foreach (K key in removals.Keys)
            {
                this.Remove(key);
            }

            return removals.Count;
        }

        public bool ContainsKey(K key)
        {
            return this.m_KeyValues.Any(x => x.Key.Equals(key));
        }

        public bool ContainsValue(T value)
        {
            return this.m_KeyValues.Any(x => x.Value.Contains(value));
        }

        public int KeyCount(K key)
        {
            return this.m_KeyValues.Count(x => x.Key.Equals(key));
        }

        public void OrderBy(Func<KeyValuePair<K, List<T>>, object> func)
        {
            this.m_KeyValues = this.m_KeyValues.OrderBy(func).ToList();
        }

        public List<T> this[K key]
        {
            get
            {
                return this.FetchValuesForKey(key);
            }
        }

        public List<K> this[T value]
        {
            get
            {
                return this.FetchKeysForValue(value);
            }
        }

        public List<T> FetchValuesForKey(K key)
        {
            List<T> values = new List<T>();

            foreach (KeyValuePair<K, List<T>> tuple in this.m_KeyValues)
            {
                if (tuple.Key.Equals(key))
                {
                    values.AddRange(tuple.Value);
                }
            }

            return values;
        }

        public List<K> FetchKeysForValue(T value)
        {
            List<K> keys = new List<K>();

            foreach (KeyValuePair<K, List<T>> tuple in this.m_KeyValues)
            {
                if (tuple.Value.Contains(value))
                {
                    keys.Add(tuple.Key);
                }
            }

            return keys;
        }

        public IEnumerator<KeyValuePair<K, List<T>>> GetEnumerator()
        {
            return this.m_KeyValues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.m_KeyValues.GetEnumerator();
        }

        public List<K> Keys
        {
            get
            {
                return this.m_KeyValues.Select(x => x.Key).ToList();
            }
        }

        public List<T> Values
        {
            get
            {
                List<T> values = new List<T>();

                foreach (K key in this.Keys)
                {
                    values.AddRange(this.FetchValuesForKey(key));
                }

                return values;
            }
        }
    }
}
