using System;
using UnityEngine;

namespace Core.Input
{
    using static CoreUtility;

    public readonly struct InputAction : IEquatable<InputAction>
    {
        internal readonly string path;

        public InputAction(string value) => path = value ?? STRING_NULL;

        public static implicit operator string(InputAction action) => action.path;

        public readonly bool GetKeyUp()
        {
            if (InputManager.HasInstance)
            {
                return InputManager.Instance.GetKeyUp(this);
            }

            return false;
        }
        public readonly bool GetKeyDown()
        {
            if (InputManager.HasInstance)
            {
                return InputManager.Instance.GetKeyDown(this);
            }

            return false;
        }
        public readonly bool GetKey()
        {
            if (InputManager.HasInstance)
            {
                return InputManager.Instance.GetKey(this);
            }

            return false;
        }
        public readonly Vector2 GetAxis()
        {
            if (InputManager.HasInstance)
            {
                return InputManager.Instance.GetAxis(this);
            }

            return Vector2.zero;
        }

        public bool Equals(InputAction other) => path == other.path;
        public override bool Equals(object obj) => obj is InputAction other && Equals(other);

        public override int GetHashCode() => path != null ? path.GetHashCode() : 0;

        public override string ToString() => path;
    }
}
