using System.ComponentModel;

namespace EncompassRest
{
    public sealed class ValuePropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public object PriorValue { get; }

        public object NewValue { get; }

        internal ValuePropertyChangedEventArgs(string propertyName, object priorValue, object newValue)
            : base(propertyName)
        {
            PriorValue = priorValue;
            NewValue = newValue;
        }
    }
}