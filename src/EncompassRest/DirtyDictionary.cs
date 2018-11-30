using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using EncompassRest.Utilities;
using Newtonsoft.Json;

namespace EncompassRest
{
    [JsonConverter(typeof(DirtyDictionaryConverter<,>))]
    internal sealed class DirtyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDirtyDictionary
    {
        internal readonly Dictionary<TKey, DirtyValue<TValue>> _dictionary;
        private ValueCollection _values;

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                var found = false;
                TValue existing = default;
                var hasHandler = CollectionChanged != null;
                if (hasHandler)
                {
                    found = _dictionary.TryGetValue(key, out var existingDirtyValue);
                    if (found)
                    {
                        existing = existingDirtyValue._value;
                    }
                }
                _dictionary[key] = value;
                if (hasHandler)
                {
                    CollectionChanged?.Invoke(this, found ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, KeyValuePair.Create(key, value), KeyValuePair.Create(key, existing)) : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, KeyValuePair.Create(key, value)));
                }
            }
        }

        public ICollection<TKey> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _values ?? (_values = new ValueCollection(this));

        public int Count => _dictionary.Count;

        public bool Dirty
        {
            get => _dictionary.Any(pair => pair.Value.Dirty);
            set
            {
                foreach (var pair in _dictionary)
                {
                    pair.Value.Dirty = value;
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        bool IDictionary.IsReadOnly => false;

        bool IDictionary.IsFixedSize => false;

        ICollection IDictionary.Keys => _dictionary.Keys;

        ICollection IDictionary.Values => (ICollection)Values;

        bool ICollection.IsSynchronized => ((ICollection)_dictionary).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

        object IDictionary.this[object key] { get => this[ValidateKey(key)]; set => this[ValidateKey(key)] = ValidateValue(value); }

        public DirtyDictionary()
            : this(0, null)
        {
        }

        public DirtyDictionary(int capacity)
            : this(capacity, null)
        {
        }

        public DirtyDictionary(IEqualityComparer<TKey> comparer)
            : this(0, comparer)
        {
        }

        public DirtyDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, DirtyValue<TValue>>(capacity, comparer);
        }

        public DirtyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this(collection, null)
        {
        }

        public DirtyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
            : this((collection as ICollection<KeyValuePair<TKey, TValue>>)?.Count ?? 0, comparer)
        {
            foreach (var pair in collection)
            {
                Add(pair.Key, pair.Value);
            }
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, KeyValuePair.Create(key, value)));
        }

        public void Clear()
        {
            _dictionary.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public Enumerator GetEnumerator() => new Enumerator(this, false);

        public bool Remove(TKey key)
        {
            TValue value = default;
            var hasHandler = CollectionChanged != null;
            if (hasHandler)
            {
                if (_dictionary.TryGetValue(key, out var dirtyValue))
                {
                    value = dirtyValue._value;
                }
                else
                {
                    return false;
                }
            }
            var success = _dictionary.Remove(key);
            if (success && hasHandler)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, KeyValuePair.Create(key, value)));
            }
            return success;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var success = _dictionary.TryGetValue(key, out var dirtyValue);
            value = success ? dirtyValue._value : default;
            return success;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => _dictionary.TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(item.Value, value);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            Preconditions.NotNull(array, nameof(array));
            Preconditions.LessThan(arrayIndex, nameof(arrayIndex), array.Length, $"{nameof(array)}.Length");
            Preconditions.GreaterThanOrEquals(array.Length - arrayIndex, $"{nameof(array)}.Length - {nameof(arrayIndex)}", Count, nameof(Count));

            var i = 0;
            foreach (var pair in _dictionary)
            {
                array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(pair.Key, pair.Value);
                ++i;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)this).Contains(item) && Remove(item.Key);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal IEnumerable<KeyValuePair<TKey, TValue>> GetDirtyItems() => _dictionary.Where(pair => pair.Value.Dirty).Select(pair => new KeyValuePair<TKey, TValue>(pair.Key, pair.Value));

        void IDictionary.Add(object key, object value) => Add(ValidateKey(key), ValidateValue(value));

        bool IDictionary.Contains(object key) => ContainsKey(ValidateKey(key));

        IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this, true);

        void IDictionary.Remove(object key) => Remove(ValidateKey(key));

        void ICollection.CopyTo(Array array, int index)
        {
            Preconditions.NotNull(array, nameof(array));
            Preconditions.LessThan(index, nameof(index), array.Length, $"{nameof(array)}.Length");
            Preconditions.GreaterThanOrEquals(array.Length - index, $"{nameof(array)}.Length - {nameof(index)}", Count, nameof(Count));

            var i = 0;
            foreach (var pair in _dictionary)
            {
                array.SetValue(new KeyValuePair<TKey, TValue>(pair.Key, pair.Value), index + i);
                ++i;
            }
        }

        private TKey ValidateKey(object key)
        {
            if (!(key is TKey tKey))
            {
                throw new ArgumentException($"key is not of type {TypeData<TKey>.Type.Name}");
            }
            return tKey;
        }

        private TValue ValidateValue(object value)
        {
            if (value == null)
            {
                return default;
            }
            if (!(value is TValue tValue))
            {
                throw new ArgumentException($"value is not of type {TypeData<TValue>.Type.Name}");
            }
            return tValue;
        }

        private sealed class ValueCollection : ICollection<TValue>, IReadOnlyCollection<TValue>, ICollection
        {
            private readonly DirtyDictionary<TKey, TValue> _dictionary;

            public int Count => _dictionary.Count;

            bool ICollection<TValue>.IsReadOnly => true;

            bool ICollection.IsSynchronized => ((ICollection)_dictionary._dictionary).IsSynchronized;

            object ICollection.SyncRoot => ((ICollection)_dictionary._dictionary).SyncRoot;

            public ValueCollection(DirtyDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                foreach (var pair in _dictionary._dictionary)
                {
                    yield return pair.Value;
                }
            }

            void ICollection<TValue>.Add(TValue item) => throw new NotSupportedException();

            void ICollection<TValue>.Clear() => throw new NotSupportedException();

            bool ICollection<TValue>.Contains(TValue item)
            {
                var comparer = EqualityComparer<TValue>.Default;
                foreach (var pair in _dictionary._dictionary)
                {
                    if (comparer.Equals(item, pair.Value))
                    {
                        return true;
                    }
                }
                return false;
            }

            void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
            {
                Preconditions.NotNull(array, nameof(array));
                Preconditions.LessThan(arrayIndex, nameof(arrayIndex), array.Length, $"{nameof(array)}.Length");
                Preconditions.GreaterThanOrEquals(array.Length - arrayIndex, $"{nameof(array)}.Length - {nameof(arrayIndex)}", Count, nameof(Count));

                var i = 0;
                foreach (var pair in _dictionary._dictionary)
                {
                    array[i] = pair.Value;
                    ++i;
                }
            }

            void ICollection.CopyTo(Array array, int index)
            {
                Preconditions.NotNull(array, nameof(array));
                Preconditions.LessThan(index, nameof(index), array.Length, $"{nameof(array)}.Length");
                Preconditions.GreaterThanOrEquals(array.Length - index, $"{nameof(array)}.Length - {nameof(index)}", Count, nameof(Count));

                var i = 0;
                foreach (var pair in _dictionary._dictionary)
                {
                    array.SetValue((TValue)pair.Value, i);
                    ++i;
                }
            }

            bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<TKey, DirtyValue<TValue>>> _enumerator;
            private readonly bool _dictionaryEnumerator;

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(_enumerator.Current.Key, _enumerator.Current.Value);

            DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(Current.Key, Current.Value);

            object IDictionaryEnumerator.Key => Current.Key;

            object IDictionaryEnumerator.Value => Current.Value;

            object IEnumerator.Current => _dictionaryEnumerator ? ((IDictionaryEnumerator)this).Entry : (object)Current;

            internal Enumerator(DirtyDictionary<TKey, TValue> dictionary, bool dictionaryEnumerator)
            {
                _enumerator = dictionary._dictionary.GetEnumerator();
                _dictionaryEnumerator = dictionaryEnumerator;
            }

            public void Dispose() => _enumerator.Dispose();

            public bool MoveNext() => _enumerator.MoveNext();

            void IEnumerator.Reset() => _enumerator.Reset();
        }
    }

    internal sealed class DirtyDictionaryConverter<TKey, TValue> : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == TypeData<DirtyDictionary<TKey, TValue>>.Type;

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotSupportedException();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dirtyDictionary = (DirtyDictionary<TKey, TValue>)value;
            var dirtyItems = dirtyDictionary.GetDirtyItems();
            var dictionary = new Dictionary<TKey, TValue>(dirtyDictionary._dictionary.Comparer);
            foreach (var dirtyItem in dirtyItems)
            {
                dictionary[dirtyItem.Key] = dirtyItem.Value;
            }
            serializer.Serialize(writer, dictionary);
        }
    }
}