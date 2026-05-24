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
        public readonly Vector3 Position;
        public readonly string Text;
        public readonly float Duration;

        public UIWaypointData(int id, Sprite icon, Color color, string text, float duration)
        {
            if (icon == null) throw new ArgumentNullException(nameof(icon));
            if (text == null) throw new ArgumentNullException(nameof(text));

            ID = id;
            Icon = icon;
            Color = color;
            Text = text;
            Duration = duration;
        }
        public UIWaypointData(int id, Transform target, Sprite icon, Color color, string text, float duration) : this (id, icon, color, text, duration)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            Target = target;
        }
        public UIWaypointData(int id, Vector3 target, Sprite icon, Color color, string text, float duration) : this (id, icon, color, text, duration)
        {
            Position = target;
        }
    }
}
