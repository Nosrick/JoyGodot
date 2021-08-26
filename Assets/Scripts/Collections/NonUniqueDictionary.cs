using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.Collections
{
    [Serializable]
    public class NonUniqueDictionary<K, T> : IEnumerable<Tuple<K, T>>
    {
        protected List<Tuple<K, T>> m_KeyValues;

        public NonUniqueDictionary()
        {
            this.m_KeyValues = new List<Tuple<K, T>>();
        }

        public NonUniqueDictionary(int capacity)
        {
            this.m_KeyValues = new List<Tuple<K, T>>(capacity);
        }

        public NonUniqueDictionary(NonUniqueDictionary<K, T> collection)
        {
            this.m_KeyValues = new List<Tuple<K, T>>(collection.m_KeyValues);
        }

        public NonUniqueDictionary(IEnumerable<Tuple<K, T>> collection)
        {
            this.m_KeyValues = new List<Tuple<K, T>>();
            this.AddRange(collection);
        }

        public void Add(K key, T value)
        {
            this.m_KeyValues.Add(new Tuple<K, T>(key, value));
        }

        public void AddRange(IEnumerable<Tuple<K, T>> values)
        {
            foreach (Tuple<K, T> value in values)
            {
                this.m_KeyValues.Add(value);
            }
        }

        public int RemoveByKey(K key, IEqualityComparer<K> comparer = default)
        {
            return comparer is null
                ? this.m_KeyValues.RemoveAll(tuple => tuple.Item1.Equals(key))
                : this.m_KeyValues.RemoveAll(tuple => comparer.Equals(tuple.Item1, key));
        }

        public int RemoveByValue(T value, IEqualityComparer<T> comparer = default)
        {
            return comparer is null
                ? this.m_KeyValues.RemoveAll(tuple => tuple.Item2.Equals(value))
                : this.m_KeyValues.RemoveAll(tuple => comparer.Equals(tuple.Item2, value));
        }

        public bool Remove(
            K key,
            T value,
            IEqualityComparer<K> keyComparer = default,
            IEqualityComparer<T> valueComparer = default)
        {
            var item = this.m_KeyValues.FirstOrDefault(tuple =>
                keyComparer is null
                    ? tuple.Item1.Equals(key)
                    : keyComparer.Equals(key, tuple.Item1)
                      &&
                      valueComparer is null
                        ? tuple.Item2.Equals(value)
                        : valueComparer.Equals(value, tuple.Item2));
            return this.m_KeyValues.Remove(item);
        }

        public bool ContainsKey(K key, IEqualityComparer<K> comparer = default)
        {
            return comparer is null
                ? this.m_KeyValues.Any(x => x.Item1.Equals(key))
                : this.m_KeyValues.Any(x => comparer.Equals(x.Item1, key));
        }

        public bool ContainsValue(T value, IEqualityComparer<T> comparer = default)
        {
            return comparer is null
                ? this.m_KeyValues.Any(x => x.Item2.Equals(value))
                : this.m_KeyValues.Any(x => comparer.Equals(x.Item2, value));
        }

        public int KeyCount(K key, IEqualityComparer<K> comparer = default)
        {
            return comparer is null
                ? this.m_KeyValues.Count(x => x.Item1.Equals(key))
                : this.m_KeyValues.Count(x => comparer.Equals(x.Item1, key));
        }

        public void OrderBy(Func<Tuple<K, T>, object> func)
        {
            this.m_KeyValues = this.m_KeyValues.OrderBy(func).ToList();
        }

        public List<T> this[K key]
        {
            get { return this.FetchValuesForKey(key); }
        }

        public List<T> FetchValuesForKey(K key, IEqualityComparer<K> comparer = default)
        {
            List<T> values = new List<T>();

            bool comparerPresent = comparer is null == false;

            foreach (Tuple<K, T> tuple in this.m_KeyValues)
            {
                if (!comparerPresent)
                {
                    if (tuple.Item1.Equals(key))
                    {
                        values.Add(tuple.Item2);
                    }
                }
                else
                {
                    if (comparer.Equals(key, tuple.Item1))
                    {
                        values.Add(tuple.Item2);
                    }
                }
            }

            return values;
        }

        public List<K> FetchKeysForValue(T value, IEqualityComparer<T> comparer = default)
        {
            List<K> keys = new List<K>();

            bool comparerPresent = comparer is null == false;

            foreach (Tuple<K, T> tuple in this.m_KeyValues)
            {
                if (!comparerPresent)
                {
                    if (tuple.Item2.Equals(value))
                    {
                        keys.Add(tuple.Item1);
                    }
                }
                else
                {
                    if (comparer.Equals(value, tuple.Item2))
                    {
                        keys.Add(tuple.Item1);
                    }
                }
            }

            return keys;
        }

        public IEnumerator<Tuple<K, T>> GetEnumerator()
        {
            return this.m_KeyValues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.m_KeyValues.GetEnumerator();
        }

        public List<Tuple<K, T>> Collection
        {
            get { return this.m_KeyValues.ToList(); }
        }

        public List<K> Keys
        {
            get { return this.m_KeyValues.Select(x => x.Item1).ToList(); }
        }

        public List<T> Values
        {
            get { return this.m_KeyValues.Select(x => x.Item2).ToList(); }
        }

        public NonUniqueDictionary<K, T> Copy()
        {
            NonUniqueDictionary<K, T> clone = new NonUniqueDictionary<K, T>();
            foreach (Tuple<K, T> tuple in this)
            {
                clone.Add(
                    ObjectExtensions.Copy(tuple.Item1),
                    ObjectExtensions.Copy(tuple.Item2));
            }

            return clone;
        }
    }
}