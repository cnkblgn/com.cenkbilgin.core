using UnityEngine;

namespace Core.Race
{
    public abstract class RaceComponent : ScriptableObject
    {
        public static RaceComponent NONE
        {
            get
            {
                if (none == null)
                {
                    none = CreateInstance<RaceComponentNone>();
                }

                return none;
            }
        } private static RaceComponentNone none;
    }

    internal sealed class RaceComponentNone : RaceComponent { }
}