using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using EncompassRest.Loans;
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
        private Dictionary<string, AttributeChangingWrapper> _propertiesAttributeLookup;

        internal int IncrementListeners()
        {
            var newNumberOfListeners = Interlocked.Increment(ref _listeners);
            if (newNumberOfListeners == 1)
            {
                _propertiesAttributeLookup = new Dictionary<string, AttributeChangingWrapper>();
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
                        AttributeChangingWrapper wrapper;
                        switch (propertyValue)
                        {
                            case DirtyExtensibleObject dirtyExtensibleObject:
                                wrapper = new AttributeChangingWrapper(this, propertyName);
                                dirtyExtensibleObject.AttributeChanging += wrapper.OnAttributeChanging;
                                _propertiesAttributeLookup.Add(propertyName, wrapper);
                                dirtyExtensibleObject.IncrementListeners();
                                break;
                            case IEnumerable<DirtyExtensibleObject> list when list is IDirtyList dirtyList:
                                wrapper = new AttributeChangingWrapper(this, propertyName, dirtyList);
                                _propertiesAttributeLookup.Add(propertyName, wrapper);
                                foreach (var item in list)
                                {
                                    item.AttributeChanging += wrapper.OnAttributeChanging;
                                    item.IncrementListeners();
                                }
                                dirtyList.CollectionChanging += wrapper.OnCollectionChanging;
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
                        AttributeChangingWrapper wrapper;
                        switch (propertyValue)
                        {
                            case DirtyExtensibleObject dirtyExtensibleObject:
                                dirtyExtensibleObject.AttributeChanging -= _propertiesAttributeLookup[propertyName].OnAttributeChanging;
                                dirtyExtensibleObject.DecrementListeners();
                                break;
                            case IEnumerable<DirtyExtensibleObject> list when list is IDirtyList dirtyList:
                                wrapper = _propertiesAttributeLookup[propertyName];
                                foreach (var item in list)
                                {
                                    item.AttributeChanging -= wrapper.OnAttributeChanging;
                                    item.DecrementListeners();
                                }
                                dirtyList.CollectionChanging -= wrapper.OnCollectionChanging;
                                dirtyList.CollectionChanged -= wrapper.OnCollectionChanged;
                                break;
                        }
                    }
                }
                _propertiesAttributeLookup = null;
            }
            return newNumberOfListeners;
        }

        // Class to propogate changes back to the Loan object and build up the model path.
        internal sealed class AttributeChangingWrapper
        {
            public DirtyExtensibleObject Source { get; }

            public string PropertyName { get; }

            public IDirtyList List { get; }

            public AttributeChangingWrapper(DirtyExtensibleObject source, string propertyName, IDirtyList list = null)
            {
                Source = source;
                PropertyName = propertyName;
                List = list;
            }

            internal void OnAttributeChanging(object sender, AttributeChangingEventArgs e)
            {
                var next = PropertyName;
                if (List != null)
                {
                    var index = List.IndexOf(sender);
                    next += $"[{index}]";
                }
                e.Path.Push(next);
                Source.AttributeChanging?.Invoke(Source, e);
            }

            internal void OnCollectionChanging(object sender, AttributeChangingEventArgs e)
            {
                e.Path.Push(PropertyName);
                Source.AttributeChanging?.Invoke(Source, e);
            }

            internal void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Replace:
                        // Supports only one item
                        var oldItem = e.OldItems[0] as DirtyExtensibleObject;
                        if (oldItem != null)
                        {
                            oldItem.AttributeChanging -= OnAttributeChanging;
                            oldItem.DecrementListeners();
                        }
                        var newItem = e.NewItems[0] as DirtyExtensibleObject;
                        if (newItem != null)
                        {
                            newItem.AttributeChanging += OnAttributeChanging;
                            newItem.IncrementListeners();
                        }
                        break;
                    case NotifyCollectionChangedAction.Add:
                        // Supports only one item
                        var addedItem = e.NewItems[0] as DirtyExtensibleObject;
                        if (addedItem != null)
                        {
                            addedItem.AttributeChanging += OnAttributeChanging;
                            addedItem.IncrementListeners();
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        // Supports multiple items
                        foreach (var item in e.OldItems.Cast<DirtyExtensibleObject>())
                        {
                            if (item != null)
                            {
                                item.AttributeChanging -= OnAttributeChanging;
                                item.DecrementListeners();
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// The property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        internal virtual void OnPropertyChanged(string propertyName, object priorValue, object newValue)
        {
            PropertyChanged?.Invoke(this, new ValuePropertyChangedEventArgs(propertyName, priorValue, newValue));
        }

        internal void ClearPropertyChangedEvent() => PropertyChanged = null;

        internal event EventHandler<AttributeChangingEventArgs> AttributeChanging;

        internal void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            var existing = field;
            var equals = EqualityComparer<T>.Default.Equals(existing, value);
            if (equals)
            {
                field = value;
            }
            else
            {
                if (AttributeChanging != null)
                {
                    var attributeChangingEventArgs = new AttributeChangingEventArgs(propertyName);
                    AttributeChanging?.Invoke(this, attributeChangingEventArgs);
                    var fieldValues = attributeChangingEventArgs.GetFieldValues(true);
                    field = value;
                    attributeChangingEventArgs.CheckForFieldChange(fieldValues);
                }
                else
                {
                    field = value;
                }
                UpdateListeners(propertyName, existing, value);
                OnPropertyChanged(propertyName, existing, value);
            }
        }

        internal void SetField<T>(ref NeverSerializeValue<T> field, T value, [CallerMemberName] string propertyName = null)
        {
            T existing = field;
            var equals = EqualityComparer<T>.Default.Equals(existing, value);
            if (equals)
            {
                field = value;
            }
            else
            {
                if (AttributeChanging != null)
                {
                    var attributeChangingEventArgs = new AttributeChangingEventArgs(propertyName);
                    AttributeChanging?.Invoke(this, attributeChangingEventArgs);
                    var fieldValues = attributeChangingEventArgs.GetFieldValues(false);
                    field = value;
                    attributeChangingEventArgs.CheckForFieldChange(fieldValues);
                }
                else
                {
                    field = value;
                }
                UpdateListeners(propertyName, existing, value);
                OnPropertyChanged(propertyName, existing, value);
            }
        }

        internal void SetField<T>(ref DirtyValue<T> field, T value, [CallerMemberName] string propertyName = null)
        {
            T existing = field;
            var equals = EqualityComparer<T>.Default.Equals(existing, value);
            if (equals)
            {
                field = value;
            }
            else
            {
                if (AttributeChanging != null)
                {
                    var attributeChangingEventArgs = new AttributeChangingEventArgs(propertyName);
                    AttributeChanging?.Invoke(this, attributeChangingEventArgs);
                    var fieldValues = attributeChangingEventArgs.GetFieldValues(false);
                    field = value;
                    attributeChangingEventArgs.CheckForFieldChange(fieldValues);
                }
                else
                {
                    field = value;
                }
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
                    existingObj.AttributeChanging -= wrapper.OnAttributeChanging;
                    if (value is DirtyExtensibleObject valueObj)
                    {
                        valueObj.AttributeChanging += wrapper.OnAttributeChanging;
                        valueObj.IncrementListeners();
                    }
                    else
                    {
                        _propertiesAttributeLookup.Remove(propertyName);
                    }
                }
                else if (value is DirtyExtensibleObject valueObj)
                {
                    var wrapper = new AttributeChangingWrapper(this, propertyName);
                    _propertiesAttributeLookup.Add(propertyName, wrapper);
                    valueObj.AttributeChanging += wrapper.OnAttributeChanging;
                    valueObj.IncrementListeners();
                }
            }
        }

        internal void SetField<T>(ref DirtyList<T> field, IList<T> value, [CallerMemberName] string propertyName = null)
        {
            var existing = field;
            if (!ReferenceEquals(existing, value))
            {
                if (AttributeChanging != null)
                {
                    var attributeChangingEventArgs = new AttributeChangingEventArgs(propertyName);
                    AttributeChanging?.Invoke(this, attributeChangingEventArgs);
                    var fieldValues = attributeChangingEventArgs.GetFieldValues(true);
                    field = value != null ? new DirtyList<T>(value) : null;
                    attributeChangingEventArgs.CheckForFieldChange(fieldValues);
                }
                else
                {
                    field = value != null ? new DirtyList<T>(value) : null;
                }
                if (_listeners > 0)
                {
                    if (_propertiesAttributeLookup.TryGetValue(propertyName, out var wrapper))
                    {
                        if (existing is IEnumerable<DirtyExtensibleObject> existingList)
                        {
                            foreach (var item in existingList)
                            {
                                item.DecrementListeners();
                                item.AttributeChanging -= wrapper.OnAttributeChanging;
                            }
                            existing.CollectionChanging -= wrapper.OnCollectionChanging;
                            existing.CollectionChanged -= wrapper.OnCollectionChanged;
                        }
                        if (value != null)
                        {
                            foreach (var item in field as IEnumerable<DirtyExtensibleObject>)
                            {
                                item.AttributeChanging += wrapper.OnAttributeChanging;
                                item.IncrementListeners();
                            }
                            field.CollectionChanging += wrapper.OnCollectionChanging;
                            field.CollectionChanged += wrapper.OnCollectionChanged;
                        }
                    }
                    else if (value != null)
                    {
                        wrapper = new AttributeChangingWrapper(this, propertyName, field);
                        _propertiesAttributeLookup.Add(propertyName, wrapper);
                        foreach (var item in field as IEnumerable<DirtyExtensibleObject>)
                        {
                            item.AttributeChanging += wrapper.OnAttributeChanging;
                            item.IncrementListeners();
                        }
                        field.CollectionChanging += wrapper.OnCollectionChanging;
                        field.CollectionChanged += wrapper.OnCollectionChanged;
                    }
                }
                OnPropertyChanged(propertyName, existing, field);
            }
        }

        internal void SetField<T>(ref DirtyDictionary<string, T> field, IDictionary<string, T> value, [CallerMemberName] string propertyName = null)
        {
            var existing = field;
            if (base.SetField(ref field, value))
            {
                if (AttributeChanging != null)
                {
                    var attributeChangingEventArgs = new AttributeChangingEventArgs(propertyName);
                    AttributeChanging?.Invoke(this, attributeChangingEventArgs);
                    var fieldValues = attributeChangingEventArgs.GetFieldValues(true);
                    field = value != null ? new DirtyDictionary<string, T>(value, StringComparer.OrdinalIgnoreCase) : null;
                    attributeChangingEventArgs.CheckForFieldChange(fieldValues);
                }
                else
                {
                    field = value != null ? new DirtyDictionary<string, T>(value, StringComparer.OrdinalIgnoreCase) : null;
                }
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
                    var wrapper = new AttributeChangingWrapper(this, propertyName);
                    AttributeChanging += wrapper.OnAttributeChanging;
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
                        wrapper = new AttributeChangingWrapper(this, propertyName, fieldValue);
                        _propertiesAttributeLookup.Add(propertyName, wrapper);
                    }
                    fieldValue.CollectionChanging += wrapper.OnCollectionChanging;
                    fieldValue.CollectionChanged += wrapper.OnCollectionChanged;
                }
                field = fieldValue;
            }
            return fieldValue;
        }

        internal IDictionary<string, T> GetField<T>(ref DirtyDictionary<string, T> field, [CallerMemberName] string propertyName = null) => base.GetField(ref field);

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
                    var extensionData = _extensionData;
                    if (extensionData != null)
                    {
                        extensionData.Dirty = value;
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

    internal sealed class AttributeChangingEventArgs : EventArgs
    {
        public Stack<string> Path { get; }

        public LoanEntity? LoanEntity { get; set; }

        public Loan Loan { get; set; }

        public AttributeChangingEventArgs()
        {
            Path = new Stack<string>();
        }

        public AttributeChangingEventArgs(string propertyName)
            : this()
        {
            Path.Push(propertyName);
        }

        public List<FieldDescriptorAndValue> GetFieldValues(bool checkChildEntities)
        {
            List<FieldDescriptorAndValue> fieldValues = null;
            if (LoanEntity.HasValue)
            {
                Traverse(LoanEntity.GetValueOrDefault());
            }
            return fieldValues;

            void Traverse(LoanEntity entity)
            {
                if (LoanFieldDescriptors.FieldMappings._entityFields.TryGetValue(entity, out var dictionary))
                {
                    if (fieldValues == null)
                    {
                        fieldValues = new List<FieldDescriptorAndValue>();
                    }
                    foreach (var pair in dictionary)
                    {
                        fieldValues.Add(new FieldDescriptorAndValue(pair.Value, pair.Value._modelPath.GetValue(Loan)));
                    }
                }
                if (checkChildEntities && LoanFieldDescriptors.s_childEntities.TryGetValue(entity, out var childEntities))
                {
                    foreach (var childEntity in childEntities)
                    {
                        Traverse(childEntity);
                    }
                }
            }
        }

        public void CheckForFieldChange(List<FieldDescriptorAndValue> originalValues)
        {
            if (originalValues != null)
            {
                var comparer = EqualityComparer<object>.Default;
                foreach (var descriptorAndValue in originalValues)
                {
                    var descriptor = descriptorAndValue.Descriptor;
                    var currentValue = descriptor._modelPath.GetValue(Loan);
                    if (!comparer.Equals(currentValue, descriptorAndValue.Value))
                    {
                        Loan.InvokeFieldChange(new FieldChangeEventArgs(descriptor.FieldId, new ReadOnlyLoanField(descriptor, Loan, descriptorAndValue.Value), new ReadOnlyLoanField(descriptor, Loan, currentValue)));
                    }
                }
            }
        }
    }

    internal struct FieldDescriptorAndValue
    {
        public FieldDescriptor Descriptor { get; }

        public object Value { get; }

        public FieldDescriptorAndValue(FieldDescriptor descriptor, object value)
        {
            Descriptor = descriptor;
            Value = value;
        }
    }
}