using System.Collections;
using System.Collections.Specialized;

namespace EncompassRest
{
    internal interface IDirtyList : IList, IDirty, INotifyCollectionChanged
    {
        int IndexOf(string id);
    }
}