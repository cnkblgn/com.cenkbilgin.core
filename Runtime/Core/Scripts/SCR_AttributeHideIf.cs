using System;

namespace Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HideIf : ShowIf
    {
        public HideIf(string conditionMethod) : base(conditionMethod, inverse: true) { }
    }
}
