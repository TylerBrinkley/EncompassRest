using System.Collections.Generic;

namespace EncompassRest.Webhook
{
    /// <summary>
    /// WebhookFilters
    /// </summary>
    [Entity(PropertiesToAlwaysSerialize = nameof(Attributes))]
    public sealed class WebhookFilters : DirtyExtensibleObject, IDirty
    {
        private DirtyList<string> _attributes;

        /// <summary>
        /// List of attribute paths to which to subscribe.
        /// </summary>
        public IList<string> Attributes { get => GetField(ref _attributes); set => SetField(ref _attributes, value); }

        bool IDirty.Dirty { get => true; set { } }
    }
}