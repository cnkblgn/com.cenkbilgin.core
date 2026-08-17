using System;

namespace Core.Mod
{
    internal sealed class AddonLoaderEntry
    {
        public readonly Type Type;
        public readonly IAddonLoader Loader;
        public readonly int Priority;

        public AddonLoaderEntry(Type type, IAddonLoader loader, int priority)
        {
            Type = type;
            Loader = loader;
            Priority = priority;
        }
    }
}
