using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using EncompassRest.Schema;
using EncompassRest.Utilities;
using Newtonsoft.Json.Linq;

namespace EncompassRest.Loans
{
    /// <summary>
    /// The loan field.
    /// </summary>
    public class LoanField : ReadOnlyLoanField
    {
        [Obsolete("Use LoanField.Descriptor.MultiInstance instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool MultiInstance => Descriptor.MultiInstance;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        [Obsolete("Use LoanField.Descriptor.InstanceSpecifier instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string InstanceSpecifier => Descriptor.InstanceSpecifier;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        [Obsolete("Use LoanField.Descriptor.IsBorrowerPairSpecific instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool IsBorrowerPairSpecific => Descriptor.IsBorrowerPairSpecific;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        [Obsolete("Use LoanField.Descriptor.ValueType instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public LoanFieldValueType ValueType => Descriptor.ValueType;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        [Obsolete("Use LoanField.Descriptor.Type instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public LoanFieldType Type => Descriptor.Type;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        [Obsolete("Use LoanField.Descriptor.Description instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string Description => Descriptor.Description;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Indicates if the field is read only.
        /// </summary>
        public bool ReadOnly => Descriptor.ReadOnly;

        /// <summary>
        /// The field's value.
        /// </summary>
        public new object Value
        {
            get => GetValue();
            set => SetValue(value is ReadOnlyLoanField field ? field.Value : value);
        }

        internal override object GetValue()
        {
            var result = _modelPath.GetValue(Loan, out _);
            return result is JValue jValue ? jValue.Value : result;
        }

        internal virtual void SetValue(object value)
        {
            if (ReadOnly)
            {
                throw new InvalidOperationException($"cannot set value of field '{FieldId}' as it's read-only");
            }
            _modelPath.SetValue(Loan, propertyType =>
            {
                if (propertyType != null)
                {
                    if (value != null && propertyType == TypeData<bool?>.Type && value is string str)
                    {
                        var result = ToBoolean(str);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                    if (value != null && (propertyType == TypeData<string>.Type || propertyType == TypeData<DateTime?>.Type || propertyType == TypeData<decimal?>.Type || propertyType == TypeData<int?>.Type || propertyType == TypeData<bool?>.Type))
                    {
                        return Convert.ChangeType(value, System.Nullable.GetUnderlyingType(propertyType) ?? propertyType);
                    }
                    else
                    {
                        var propertyTypeContract = JsonHelper.InternalPrivateContractResolver.ResolveContract(propertyType);
                        if (propertyTypeContract.Converter is IStringCreator stringCreator)
                        {
                            return stringCreator.Create(value?.ToString());
                        }
                    }
                }
                return value;
            });
        }

        /// <summary>
        /// The field's locked status.
        /// </summary>
        public virtual bool Locked
        {
            get
            {
                var fieldLockData = Loan.FieldLockData.GetById(ModelPath);
                return fieldLockData != null && fieldLockData.LockRemoved != true;
            }
            set
            {
                var allFieldLockData = Loan.FieldLockData;
                var modelPath = ModelPath;
                var fieldLockData = allFieldLockData.GetById(modelPath);
                if (fieldLockData == null)
                {
                    fieldLockData = new FieldLockData { ModelPath = modelPath };
                    allFieldLockData.Add(fieldLockData);
                }
                fieldLockData.LockRemoved = !value;
            }
        }

        [Obsolete("Use LoanField.Descriptor.Options instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ReadOnlyCollection<FieldOption> Options => Descriptor.Options;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        internal LoanField(FieldDescriptor descriptor, Loan loan, ModelPath modelPath = null, int? borrowerPairIndex = null)
            : base(descriptor, loan, null, modelPath, borrowerPairIndex)
        {
        }
    }
}