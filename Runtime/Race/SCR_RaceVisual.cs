using UnityEngine;

namespace Core.Race
{
    public abstract class RaceVisual : ScriptableObject
    {
        public static RaceVisual NONE
        {
            get
            {
                if (none == null)
                {
                    none = CreateInstance<RaceVisualNone>();
                }

                return none;
            }
        } private static RaceVisualNone none;
    }

    internal sealed class RaceVisualNone : RaceVisual { }
}