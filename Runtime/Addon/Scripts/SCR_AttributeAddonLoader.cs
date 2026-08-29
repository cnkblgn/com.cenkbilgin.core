using System;

namespace Core.Addon
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AddonLoader : Attribute
    {
        public int Priority { get; }

        public AddonLoader(int priority = 0)
        {
            Priority = priority;
        }
    }
}
