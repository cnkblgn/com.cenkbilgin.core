using System;

namespace Core.UI
{
    public readonly struct UIViewportEntry
    {
        public readonly Guid InstanceID;
        public readonly UIViewportItem Item;

        public UIViewportEntry(Guid instanceID, UIViewportItem item)
        {
            InstanceID = instanceID == Guid.Empty ? throw new ArgumentNullException(nameof(instanceID)) : instanceID;
            Item = item != null ? item : throw new ArgumentNullException(nameof(item));
        }
    }
}