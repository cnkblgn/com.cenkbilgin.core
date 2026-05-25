using System;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    public readonly struct UIWaypointData
    {
        public static readonly UIWaypointData Empty = new(-1);
        public readonly Vector3 Position => TargetTransform != null ? TargetTransform.position : TargetPosition;

        public readonly int ID;
        public readonly Transform TargetTransform;
        public readonly Sprite Icon;
        public readonly Vector3 TargetPosition;
        public readonly Color Color;
        public readonly string Text;
        public readonly float Duration;
        public readonly bool HasTarget;

        public UIWaypointData(int id)
        {
            ID = id;
            TargetTransform = null;
            Icon = null;
            Color = COLOR_WHITE;
            TargetPosition = Vector3.zero;
            Text = null;
            Duration = 0;
            HasTarget = false;
        }
        public UIWaypointData(int id, Sprite icon, Color color, string text, float duration)
        {
            if (icon == null) throw new ArgumentNullException(nameof(icon));
            if (text == null) throw new ArgumentNullException(nameof(text));

            TargetTransform = null;
            TargetPosition = Vector3.zero;

            ID = id;
            Icon = icon;
            Color = color;
            Text = text;
            Duration = duration;
            HasTarget = false;
        }
        public UIWaypointData(int id, Transform target, Sprite icon, Color color, string text, float duration) : this (id, icon, color, text, duration)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            TargetTransform = target;
            HasTarget = true;
        }
        public UIWaypointData(int id, Vector3 target, Sprite icon, Color color, string text, float duration) : this (id, icon, color, text, duration)
        {
            TargetPosition = target;
        }
    }
}
