using System;
using UnityEngine;

namespace Core.Damage
{
    [Serializable]
    public struct DamageProtection
    {
        public int ID;
        public DamageTag Tag;
        [Range(0, 1)] public float Percent;

        public DamageProtection(int id, DamageTag tag, float percent)
        {
            ID = id;
            Tag = tag;
            Percent = percent;
        }
    }
}