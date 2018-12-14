using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using EncompassRest.Utilities;
using Newtonsoft.Json.Serialization;

namespace EncompassRest
{
    /// <summary>
    /// Base class that supports extension data and dirty json serialization.
    /// </summary>
    public abstract class DirtyExtensibleObject : ExtensibleObject, IDirty, IIdentifiable, INotifyPropertyChanged
#if HAVE_ICLONEABLE
        , ICloneable
#endif
    {
        private int _listeners;
        private Dictionary<string, AttributeChangedWrapper> _propertiesAttributeLookup;

        internal int IncrementListeners()
        {
            var newNumberOfListeners = Interlocked.Increment(ref _listeners);
            if (newNumberOfListeners == 1)
            {
                _propertiesAttributeLookup = new Dictionary<string, AttributeChangedWrapper>();
                var customContractResolver = JsonHelper.InternalPrivateContractResolver;
                var contract = (JsonObjectContract)customContractResolver.ResolveContract(GetType());
                foreach (var property in contract.Properties)
                {
                    if (!property.Ignored)
                    {
                        var propertyName = property.UnderlyingName;
                        var valueProvider = customContractResolver.GetBackingFieldInfo(property.DeclaringType, propertyName)?.ValueProvider ?? property.ValueProvider;
                        var propertyValue = valueProvider.GetValue(this);
                        if (propertyValue is IValue v)
                        {
                            propertyValue = v.Value;
                        }
                        AttributeChangedWrapper wrapper;
                        switch (propertyValue)
                        {
                            case DirtyExtensibleObject dirtyExtensibleObject:
                                wrapper = new AttributeChangedWrapper(this, propertyName);
                                dirtyExtensibleObject.AttributeChanged += wrapper.OnAttributeChanged;
                                _propertiesAttributeLookup.Add(propertyName, wrapper);
                                dirtyExtensibleObject.IncrementListeners();
                                break;
                            case IEnumerable<DirtyExtensibleObject> list when list is IDirtyList dirtyList:
                                if (!_propertiesAttributeLookup.TryGetValue(propertyName, out wrapper))
                                {
                                    wrapper = new AttributeChangedWrapper(this, propertyName, dirtyList);
                                    _propertiesAttributeLookup.Add(propertyName, wrapper);
                                }
                                foreach (var item in list)
                                {
                                    item.AttributeChanged += wrapper.OnAttributeChanged;
                                    item.IncrementListeners();
                                }
                                dirtyList.CollectionChanged += wrapper.OnCollectionChanged;
                                break;
                        }
                    }
                }
            }
            return newNumberOfListeners;
        }

        internal int DecrementListeners()
        {
            var newNumberOfListeners = Interlocked.Decrement(ref _listeners);
            if (newNumberOfListeners == 0)
            {
                var customContractResolver = JsonHelper.InternalPrivateContractResolver;
                var contract = (JsonObjectContract)customContractResolver.ResolveContract(GetType());
                foreach (var property in contract.Properties)
                {
                    if (!property.Ignored)
                    {
                        var propertyName = property.UnderlyingName;
                        var valueProvider = customContractResolver.GetBackingFieldInfo(property.DeclaringType, propertyName)?.ValueProvider ?? property.ValueProvider;
                        var propertyValue = valueProvider.GetValue(this);
                        if (propertyValue is IValue v)
                        {
                            propertyValue = v.Value;
                        }
                        AttributeChangedWrapper wrapper;
                        switch (propertyValue)
                        {
                            case DirtyExtensibleObject dirtyExtensibleObject:
                                dirtyExtensibleObject.AttributeChanged -= _propertiesAttributeLookup[propertyName].OnAttributeChanged;
                                dirtyExtensibleObject.DecrementListeners();
                                break;
                            case IEnumerable<DirtyExtensibleObject> list when list is IDirtyList dirtyList:
                                if (!_propertiesAttributeLookup.TryGetValue(propertyName, out wrapper))
                                {
                                    wrapper = new AttributeChangedWrapper(this, propertyName, dirtyList);
                                    _propertiesAttributeLookup.Add(propertyName, wrapper);
                                }
                                foreach (var item in list)
                                {
                                    item.AttributeChanged -= wrapper.OnAttributeChanged;
                                    item.DecrementListeners();
                                }
                                dirtyList.CollectionChanged -= wrapper.OnCollectionChanged;
                                break;
                        }
                    }
                }
                _propertiesAttributeLookup = null;
            }
            return newNumberOfListeners;
        }

        internal sealed class AttributeChangedWrapper
        {
            public DirtyExtensibleObject Source { get; }

            public string PropertyName { get; }

            public IDirtyList List { get; }

            public AttributeChangedWrapper(DirtyExtensibleObject source, string propertyName, IDirtyList list = null)
            {
                Source = source;
                PropertyName = propertyName;
                List = list;
            }

            internal void OnAttributeChanged(object sender, AttributeChangedEventArgs e)
            {
                var next = PropertyName;
                if (List != null)
                {
                    var index = List.IndexOf(sender);
                    next += $"[{index}]";
                }
                e.Path.Push(next);
                Source.AttributeChanged?.Invoke(Source, e);
            }

            internal void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                AttributeChangedEventArgs attributeChangedEventArgs = null;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Replace:
                        // Supports only one item
                        var oldItem = e.OldItems[0] as DirtyExtensibleObject;
                        if (oldItem != null)
                        {
                            oldItem.AttributeChanged -= OnAttributeChanged;
                            oldItem.DecrementListeners();
                        }
                        var newItem = e.NewItems[0] as DirtyExtensibleObject;
                        if (newItem != null)
                        {
                            newItem.AttributeChanged += OnAttributeChanged;
                            newItem.IncrementListeners();
                        }
                        attributeChangedEventArgs = AttributeChangedEventArgs.CreateReplace(PropertyName, e.OldItems, e.NewItems, e.NewStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Add:
                        // Supports only one item
                        var addedItem = e.NewItems[0] as DirtyExtensibleObject;
                        if (addedItem != null)
                        {
                            addedItem.AttributeChanged += OnAttributeChanged;
                            addedItem.IncrementListeners();
                        }
                        attributeChangedEventArgs = AttributeChangedEventArgs.CreateAdd(PropertyName, e.NewItems, e.NewStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        // Supports multiple items
                        foreach (var item in e.OldItems.Cast<DirtyExtensibleObject>())
                        {
                            if (item != null)
                            {
                                item.AttributeChanged -= OnAttributeChanged;
                                item.DecrementListeners();
                            }
                        }
                        attributeChangedEventArgs = AttributeChangedEventArgs.CreateRemove(PropertyName, e.OldItems, e.OldStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        // Supports multiple items
                        attributeChangedEventArgs = AttributeChangedEventArgs.CreateMove(PropertyName, e.OldItems, e.OldStartingIndex, e.NewStartingIndex);
                        break;
                }
                Source.AttributeChanged?.Invoke(Source, attributeChangedEventArgs);
            }
        }

        /// <summary>
        /// The PropertyChanged Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        internal virtual void OnPropertyChanged(string propertyName, object priorValue, object newValue)
        {
            PropertyChanged?.Invoke(this, new ValuePropertyChangedEventArgs(propertyName, priorValue, newValue));
            AttributeChanged?.Invoke(this, AttributeChangedEventArgs.CreateReplace(propertyName, priorValue, newValue));
        }

        internal void ClearPropertyChangedEvent() => PropertyChanged = null;

        internal event EventHandler<AttributeChangedEventArgs> AttributeChanged;

        internal void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            var existing = field;
            var equals = EqualityComparer<T>.Default.Equals(existing, value);
            field = value;
            if (!equals)
            {
                UpdateListeners(propertyName, existing, value);
                OnPropertyChanged(propertyName, existing, value);
            }
        }

        internal void SetField<T>(ref NeverSerializeValue<T> field, T value, [CallerMemberName] string propertyName = null)
        {
            T existing = field;
            var equals = EqualityComparer<T>.Default.Equals(existing, value);
            field = value;
            if (!equals)
            {
                UpdateListeners(propertyName, existing, value);
                OnPropertyChanged(propertyName, existing, value);
            }
        }

        internal void SetField<T>(ref DirtyValue<T> field, T value, [CallerMemberName] string propertyName = null)
        {
            T existing = field;
            var equals = EqualityComparer<T>.Default.Equals(existing, value);
            field = value;
            if (!equals)
            {
                UpdateListeners(propertyName, existing, value);
                OnPropertyChanged(propertyName, existing, value);
            }
        }

        private void UpdateListeners<T>(string propertyName, T existing, T value)
        {
            if (_listeners > 0)
            {
                if (existing is DirtyExtensibleObject existingObj)
                {
                    existingObj.DecrementListeners();
                    var wrapper = _propertiesAttributeLookup[propertyName];
                    existingObj.AttributeChanged -= wrapper.OnAttributeChanged;
                    if (value is DirtyExtensibleObject valueObj)
                    {
                        valueObj.AttributeChanged += wrapper.OnAttributeChanged;
                        valueObj.IncrementListeners();
                    }
                    else
                    {
                        _propertiesAttributeLookup.Remove(propertyName);
                    }
                }
                else if (value is DirtyExtensibleObject valueObj)
                {
                    var wrapper = new AttributeChangedWrapper(this, propertyName);
                    _propertiesAttributeLookup.Add(propertyName, wrapper);
                    valueObj.AttributeChanged += wrapper.OnAttributeChanged;
                    valueObj.IncrementListeners();
                }
            }
        }

        internal void SetField<T>(ref DirtyList<T> field, IList<T> value, [CallerMemberName] string propertyName = null)
        {
            var existing = field;
            if (!ReferenceEquals(existing, value))
            {
                field = value != null ? new DirtyList<T>(value) : null;
                if (_listeners > 0)
                {
                    if (_propertiesAttributeLookup.TryGetValue(propertyName, out var wrapper))
                    {
                        if (existing is IEnumerable<DirtyExtensibleObject> existingList)
                        {
                            foreach (var item in existingList)
                            {
                                item.DecrementListeners();
                                item.AttributeChanged -= wrapper.OnAttributeChanged;
                            }
                            existing.CollectionChanged -= wrapper.OnCollectionChanged;
                        }
                        if (value != null)
                        {
                            foreach (var item in field as IEnumerable<DirtyExtensibleObject>)
                            {
                                item.AttributeChanged += wrapper.OnAttributeChanged;
                                item.IncrementListeners();
                            }
                            field.CollectionChanged += wrapper.OnCollectionChanged;
                        }
                    }
                    else if (value != null)
                    {
                        wrapper = new AttributeChangedWrapper(this, propertyName, field);
                        _propertiesAttributeLookup.Add(propertyName, wrapper);
                        foreach (var item in field as IEnumerable<DirtyExtensibleObject>)
                        {
                            item.AttributeChanged += wrapper.OnAttributeChanged;
                            item.IncrementListeners();
                        }
                        field.CollectionChanged += wrapper.OnCollectionChanged;
                    }
                }
                OnPropertyChanged(propertyName, existing, field);
            }
        }

        internal void SetField<T>(ref DirtyDictionary<string, T> field, IDictionary<string, T> value, [CallerMemberName] string propertyName = null)
        {
            var existing = field;
            if (!ReferenceEquals(existing, value))
            {
                field = value != null ? new DirtyDictionary<string, T>(value, StringComparer.OrdinalIgnoreCase) : null;
                OnPropertyChanged(propertyName, existing, field);
            }
        }

        internal T GetField<T>(ref T field, [CallerMemberName] string propertyName = null)
            where T : DirtyExtensibleObject, new()
        {
            var fieldValue = field;
            if (fieldValue == null)
            {
                fieldValue = new T();
                if (_listeners > 0 && fieldValue is DirtyExtensibleObject fieldValueObj)
                {
                    fieldValueObj.IncrementListeners();
                    var wrapper = new AttributeChangedWrapper(this, propertyName);
                    AttributeChanged += wrapper.OnAttributeChanged;
                }
                field = fieldValue;
            }
            return fieldValue;
        }

        internal IList<T> GetField<T>(ref DirtyList<T> field, [CallerMemberName] string propertyName = null)
        {
            var fieldValue = field;
            if (fieldValue == null)
            {
                fieldValue = new DirtyList<T>();
                if (_listeners > 0 && fieldValue is IEnumerable<DirtyExtensibleObject> list)
                {
                    if (!_propertiesAttributeLookup.TryGetValue(propertyName, out var wrapper))
                    {
                        wrapper = new AttributeChangedWrapper(this, propertyName, fieldValue);
                        _propertiesAttributeLookup.Add(propertyName, wrapper);
                    }
                    fieldValue.CollectionChanged += wrapper.OnCollectionChanged;
                }
                field = fieldValue;
            }
            return fieldValue;
        }

        internal IDictionary<string, T> GetField<T>(ref DirtyDictionary<string, T> field, [CallerMemberName] string propertyName = null) => field ?? (field = new DirtyDictionary<string, T>(StringComparer.OrdinalIgnoreCase));

        private bool _gettingDirty;
        private bool _settingDirty;
        internal bool Dirty
        {
            get
            {
                if (_gettingDirty)
                {
                    return false;
                }
                _gettingDirty = true;
                var dirty = _extensionData?.Dirty == true;
                if (!dirty)
                {
                    var customContractResolver = JsonHelper.InternalPrivateContractResolver;
                    var contract = (JsonObjectContract)customContractResolver.ResolveContract(GetType());
                    foreach (var property in contract.Properties)
                    {
                        if (!property.Ignored)
                        {
                            var valueProvider = customContractResolver.GetBackingFieldInfo(property.DeclaringType, property.UnderlyingName)?.ValueProvider ?? property.ValueProvider;
                            if ((valueProvider.GetValue(this) as IDirty)?.Dirty == true)
                            {
                                dirty = true;
                                break;
                            }
                        }
                    }
                }
                _gettingDirty = false;
                return dirty;
            }
            set
            {
                if (!_settingDirty)
                {
                    _settingDirty = true;
                    var customContractResolver = JsonHelper.InternalPrivateContractResolver;
                    var contract = (JsonObjectContract)customContractResolver.ResolveContract(GetType());
                    foreach (var property in contract.Properties)
                    {
                        if (!property.Ignored)
                        {
                            var valueProvider = customContractResolver.GetBackingFieldInfo(property.DeclaringType, property.UnderlyingName)?.ValueProvider ?? property.ValueProvider;
                            if (valueProvider.GetValue(this) is IDirty dirtyObject)
                            {
                                dirtyObject.Dirty = value;
                            }
                        }
                    }
                    if (_extensionData != null)
                    {
                        _extensionData.Dirty = value;
                    }
                    _settingDirty = false;
                }
            }
        }
        bool IDirty.Dirty { get => Dirty; set => Dirty = value; }

        string IIdentifiable.Id { get => string.Empty; set { } }

        internal DirtyExtensibleObject()
        {
        }

        internal static string GetIdPropertyName(TypeInfo typeInfo)
        {
            var idProperty = GetIdProperty(typeInfo);
            var idPropertyNameAttribute = idProperty.GetCustomAttribute<IdPropertyNameAttribute>(false);
            return idPropertyNameAttribute != null ? idPropertyNameAttribute.IdPropertyName : "Id";
        }

        private static PropertyInfo GetIdProperty(TypeInfo typeInfo) => typeInfo.DeclaredProperties.FirstOrDefault(p => p.Name == "EncompassRest.IIdentifiable.Id") ?? typeInfo.DeclaredProperties.FirstOrDefault(p => p.Name == "Id") ?? GetIdProperty(typeInfo.BaseType.GetTypeInfo());

#if HAVE_ICLONEABLE
        object ICloneable.Clone() => this.Clone();
#endif
    }

    internal sealed class AttributeChangedEventArgs : EventArgs
    {
        public static AttributeChangedEventArgs CreateAdd(string propertyName, IList addedValues, int index) => new AttributeChangedEventArgs(propertyName, AttributeChangedAction.Add, null, addedValues, -1, index);

        public static AttributeChangedEventArgs CreateRemove(string propertyName, IList removedValues, int index) => new AttributeChangedEventArgs(propertyName, AttributeChangedAction.Remove, removedValues, null, index, -1);

        public static AttributeChangedEventArgs CreateReplace(string propertyName, object oldValue, object newValue) => new AttributeChangedEventArgs(propertyName, AttributeChangedAction.Replace, new OneItemList(oldValue), new OneItemList(newValue), -1, -1);

        public static AttributeChangedEventArgs CreateReplace(string propertyName, IList oldValues, IList newValues, int index) => new AttributeChangedEventArgs(propertyName, AttributeChangedAction.Replace, oldValues, newValues, index, index);

        public static AttributeChangedEventArgs CreateMove(string propertyName, IList items, int oldStartingIndex, int newStartingIndex) => new AttributeChangedEventArgs(propertyName, AttributeChangedAction.Move, items, items, oldStartingIndex, newStartingIndex);

        public AttributeChangedAction Action { get; }

        public IList OldValues { get; }

        public IList NewValues { get; }

        public Stack<string> Path { get; }

        public int OldStartingIndex { get; }

        public int NewStartingIndex { get; }

        private AttributeChangedEventArgs(string propertyName, AttributeChangedAction action, IList oldValues, IList newValues, int oldStartingIndex, int newStartingIndex)
        {
            Path = new Stack<string>();
            Path.Push(propertyName);
            Action = action;
            OldValues = oldValues;
            NewValues = newValues;
            OldStartingIndex = oldStartingIndex;
            NewStartingIndex = newStartingIndex;
        }

        private sealed class OneItemList : IList
        {
            private readonly object _value;

            public object this[int index]
            {
                get => index == 0 ? _value : throw new ArgumentOutOfRangeException(nameof(index));
                set => throw new NotSupportedException();
            }

            public bool IsFixedSize => true;

            public bool IsReadOnly => true;

            public int Count => 1;

            public bool IsSynchronized => true;

            public object SyncRoot => null;

            public OneItemList(object value)
            {
                _value = value;
            }

            public int Add(object value) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Contains(object value) => IndexOf(value) == 0;

            public void CopyTo(Array array, int index) => array.SetValue(_value, index);

            public IEnumerator GetEnumerator()
            {
                yield return _value;
            }

            public int IndexOf(object value)
            {
                if (value != null)
                {
                    if (_value != null)
                    {
                        return value.Equals(_value) ? 0 : -1;
                    }
                }
                else if (_value == null)
                {
                    return 0;
                }
                return -1;
            }

            public void Insert(int index, object value) => throw new NotSupportedException();

            public void Remove(object value) => throw new NotSupportedException();

            public void RemoveAt(int index) => throw new NotSupportedException();
        }
    }

    internal enum AttributeChangedAction
    {
        Add = 0,
        Remove = 1,
        Replace = 2,
        Move = 3
    }
}