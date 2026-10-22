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


        /// <summary> Called when editor validates component. </summary>
        public abstract void Validate(RaceID id);
    }

    internal sealed class RaceVisualNone : RaceVisual 
    {
        public override void Validate(RaceID id) { }
    }
}