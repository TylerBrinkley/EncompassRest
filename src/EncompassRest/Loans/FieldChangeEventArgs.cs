using System;

namespace EncompassRest.Loans
{
    public sealed class FieldChangeEventArgs : EventArgs
    {
        public string ModelPath { get; }

        public object PriorValue { get; }

        public object NewValue { get; }

        internal FieldChangeEventArgs(string modelPath, object priorValue, object newValue)
        {
            ModelPath = modelPath;
            PriorValue = priorValue;
            NewValue = newValue;
        }
    }
}