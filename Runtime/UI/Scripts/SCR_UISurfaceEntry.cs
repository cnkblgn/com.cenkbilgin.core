using System;
using UnityEngine;

namespace Core.UI
{
    public readonly struct UISurfaceEntry
    {
        public readonly Guid InstanceID;
        public readonly UISurfaceItem Item;

        public UISurfaceEntry(Guid instanceID, UISurfaceItem item)
        {
            InstanceID = instanceID == Guid.Empty ? throw new ArgumentNullException(nameof(instanceID)) : instanceID;
            Item = item != null ? item : throw new ArgumentNullException(nameof(item));
        }
    }
}