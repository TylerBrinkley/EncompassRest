using System;

namespace EncompassRest.Loans
{
    /// <summary>
    /// FieldChangeEventArgs
    /// </summary>
    public sealed class FieldChangeEventArgs : EventArgs
    {
        /// <summary>
        /// The id of the field whose value changed.
        /// </summary>
        public string FieldId { get; }

        /// <summary>
        /// The field's prior value.
        /// </summary>
        public ReadOnlyLoanField PriorValue { get; }

        /// <summary>
        /// The field's new value.
        /// </summary>
        public ReadOnlyLoanField NewValue { get; }

        internal FieldChangeEventArgs(string fieldId, ReadOnlyLoanField priorValue, ReadOnlyLoanField newValue)
        {
            FieldId = fieldId;
            PriorValue = priorValue;
            NewValue = newValue;
        }
    }
}