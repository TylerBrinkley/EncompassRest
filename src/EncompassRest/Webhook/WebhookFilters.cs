using System.Collections.Generic;

namespace EncompassRest.Webhook
{
    [Entity(PropertiesToAlwaysSerialize = nameof(Attributes))]
    public sealed class WebhookFilters : DirtyExtensibleObject, IDirty
    {
        private DirtyList<string> _attributes;

        public IList<string> Attributes { get => GetField(ref _attributes); set => SetField(ref _attributes, value); }

        bool IDirty.Dirty { get => true; set { } }
    }
}