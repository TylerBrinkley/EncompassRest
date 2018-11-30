using System;

namespace EncompassRest.Loans
{
    internal sealed class VirtualLoanField : LoanField
    {
        internal override object GetValue() => Loan.VirtualFields.TryGetValue(FieldId, out var value) ? value : null;

        internal override void SetValue(object value) => throw new InvalidOperationException($"cannot set value of field '{FieldId}' as it's virtual");

        public override bool Locked
        {
            get => false;
            set => throw new InvalidOperationException($"cannot lock/unlock field '{FieldId}' as it's virtual");
        }

        internal VirtualLoanField(FieldDescriptor descriptor, Loan loan)
            : base(descriptor, loan)
        {
        }
    }
}