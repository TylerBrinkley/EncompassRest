using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        internal int IncrementListeners(bool add)
        {
            var originalNumberOfListeners = _listeners;
            _listeners += add ? 1 : -1;
            if ((add && originalNumberOfListeners == 0) || (!add && originalNumberOfListeners == 1))
            {
                if (add)
                {
                    _propertiesAttributeLookup = new Dictionary<string, AttributeChangedWrapper>();
                }
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
                        switch (propertyValue)
                        {
                            case DirtyExtensibleObject dirtyExtensibleObject:
                                if (add)
                                {
                                    var wrapper = CreateWrapper(propertyName);
                                    dirtyExtensibleObject.AttributeChanged += wrapper.OnAttributeChanged;
                                    _propertiesAttributeLookup.Add(propertyName, wrapper);
                                }
                                else
                                {
                                    dirtyExtensibleObject.AttributeChanged -= _propertiesAttributeLookup[propertyName].OnAttributeChanged;
                                }
                                dirtyExtensibleObject.IncrementListeners(add);
                                break;
                            case IDirtyList dirtyList:
                                // TODO
                                break;
                        }
                    }
                }
                if (!add)
                {
                    _propertiesAttributeLookup = null;
                }
            }
            return originalNumberOfListeners;
        }

        internal sealed class AttributeChangedWrapper
        {
            public DirtyExtensibleObject Source { get; }

            public string PropertyName { get; }

            public AttributeChangedWrapper(DirtyExtensibleObject source, string propertyName)
            {
                Source = source;
                PropertyName = propertyName;
            }

            internal void OnAttributeChanged(object sender, AttributeChangedEventArgs e)
            {
                e.Path.Push(PropertyName);
                Source.AttributeChanged?.Invoke(Source, e);
            }
        }

        internal AttributeChangedWrapper CreateWrapper(string propertyName) => new AttributeChangedWrapper(this, propertyName);

        /// <summary>
        /// The PropertyChanged Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        internal virtual void OnPropertyChanged(string propertyName, object priorValue, object newValue)
        {
            PropertyChanged?.Invoke(this, new ValuePropertyChangedEventArgs(propertyName, priorValue, newValue));
            AttributeChanged?.Invoke(this, new AttributeChangedEventArgs(priorValue, newValue, propertyName));
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
                    existingObj.IncrementListeners(false);
                    existingObj.AttributeChanged -= _propertiesAttributeLookup[propertyName].OnAttributeChanged;
                    _propertiesAttributeLookup.Remove(propertyName);
                }
                if (value is DirtyExtensibleObject valueObj)
                {
                    var wrapper = CreateWrapper(propertyName);
                    valueObj.AttributeChanged += wrapper.OnAttributeChanged;
                    _propertiesAttributeLookup.Add(propertyName, wrapper);
                    valueObj.IncrementListeners(true);
                }
            }
        }

        internal void SetField<T>(ref DirtyList<T> field, IList<T> value, [CallerMemberName] string propertyName = null)
        {
            var existing = field;
            if (!ReferenceEquals(existing, value))
            {
                field = value != null ? new DirtyList<T>(value) : null;
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
                    fieldValueObj.IncrementListeners(true);
                    var wrapper = CreateWrapper(propertyName);
                    AttributeChanged += wrapper.OnAttributeChanged;
                }
                field = fieldValue;
            }
            return fieldValue;
        }

        internal IList<T> GetField<T>(ref DirtyList<T> field, [CallerMemberName] string propertyName = null) => field ?? (field = new DirtyList<T>());

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
        public object PriorValue { get; }

        public object NewValue { get; }

        public Stack<string> Path { get; }

        public AttributeChangedEventArgs(object priorValue, object newValue, string propertyName)
        {
            PriorValue = priorValue;
            NewValue = newValue;
            Path = new Stack<string>();
            Path.Push(propertyName);
        }
    }
}