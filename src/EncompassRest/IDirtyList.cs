using System;
using System.Collections;
using System.Collections.Specialized;

namespace EncompassRest
{
    internal interface IDirtyList : IList, IDirty, INotifyCollectionChanged
    {
        void AddRange(IList values, int start, int length);
        int IndexOf(string id);
        void Move(int fromIndex, int toIndex);
        event EventHandler<AttributeChangingEventArgs> CollectionChanging;
    }
}