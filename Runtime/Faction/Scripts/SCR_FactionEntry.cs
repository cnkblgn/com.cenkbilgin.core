using System;
using UnityEngine;
using Core.Graphics;
using Core.Localization;

namespace Core.Faction
{
    [Serializable]
    public sealed class FactionEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
#endif
        public FactionID ID;
        public FactionRelation[] Relations;
        public IconID IconID;
        public LocalizedID NameID;
        public LocalizedID DescID;

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;

            Relations = FactionRelation.GetNormalized(Relations);

            for (int i = 0; i < Relations.Length; i++)
            {
                Relations[i].OnValidate();
            }
        }
#endif
    }
}