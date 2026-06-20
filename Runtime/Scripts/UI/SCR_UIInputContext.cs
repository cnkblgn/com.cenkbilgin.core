using System;
using UnityEngine;

namespace Core.UI
{
    public readonly struct UIInputContext
    {
        public readonly Camera Camera;
        public readonly Vector2 PointerPosition;
        public readonly Vector2 PointerScroll;
        public readonly bool KeyDown;
        public readonly bool KeyUp;

        public UIInputContext(Camera camera, Vector2 pointerPosition, Vector2 pointerScroll, bool keyDown, bool keyUp)
        {
            Camera = camera != null ? camera : throw new ArgumentNullException($"[{nameof(camera)}] Camera cannot be null in input context!");
            PointerPosition = pointerPosition;
            PointerScroll = pointerScroll;
            KeyDown = keyDown;
            KeyUp = keyUp;
        }
    }
}