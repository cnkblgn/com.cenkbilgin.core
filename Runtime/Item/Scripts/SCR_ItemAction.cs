using System;
using Core.Actors;

namespace Core.Item
{
    [Serializable]
    public abstract class ItemAction
    {
        /// <summary> Returns display name </summary>
        public abstract string GetName();


        /// <summary> Applies action </summary>
        public abstract void Apply(Actor actor, ItemID id);
    }
}
