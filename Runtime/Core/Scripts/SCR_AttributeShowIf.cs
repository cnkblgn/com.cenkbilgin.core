using System;
using UnityEngine;

namespace Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ShowIf : PropertyAttribute
    {
        public string ConditionMethod;
        public bool Inverse;

        public ShowIf(string conditionMethod, bool inverse = false)
        {
            ConditionMethod = conditionMethod;
            Inverse = inverse;
        }
    }
}
