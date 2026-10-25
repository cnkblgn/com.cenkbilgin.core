using UnityEngine;

namespace Core
{
    public abstract class Compatibility : ScriptableObject
    {
        public static readonly Compatibility[] NONE = new Compatibility[0];
    }
    public abstract class Compatibility<TContext> : Compatibility
    {
        public abstract bool IsCompatibleWith(TContext ctx);
    }
}
