using System.Collections;
using System.Collections.Specialized;

namespace EncompassRest
{
    internal interface IDirtyDictionary : IDictionary, IDirty, INotifyCollectionChanged
    {
    }
}