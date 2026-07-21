using System;
using UnityEngine;

namespace Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class Clickable : PropertyAttribute
    {
        public string Label;

        public Clickable(string label = null) => this.Label = label;
    }
}
