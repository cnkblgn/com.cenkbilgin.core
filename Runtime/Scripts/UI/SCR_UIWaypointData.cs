using System;
using UnityEngine;

namespace Core.UI
{
    public sealed class UIWaypointData
    {
        public readonly int ID;
        public readonly Transform Target;
        public readonly Sprite Icon;
        public readonly Color Color;
        public readonly string Text;
        public readonly float Duration;

        public UIWaypointData(int id, Transform target, Sprite icon, Color color, Vector3 offset, string text, float duration)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (icon == null) throw new ArgumentNullException(nameof(icon));
            if (text == null) throw new ArgumentNullException(nameof(text));

            ID = id;
            Target = target;
            Icon = icon;
            Color = color;
            Text = text;
            Duration = duration;
        }
    }
}
