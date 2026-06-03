using UnityEngine;

namespace Core.Input
{
    using static CoreUtility;

    public static class InputUtility
    {
        public static bool GetKeyUp(this InputAction type)
        {
            if (ManagerCoreInput.Instance != null)
            {
                return ManagerCoreInput.Instance.GetKeyUp(type);
            }

            return false;
        }
        public static bool GetKeyDown(this InputAction type)
        {
            if (ManagerCoreInput.Instance != null)
            {
                return ManagerCoreInput.Instance.GetKeyDown(type);
            }

            return false;
        }
        public static bool GetKey(this InputAction type)
        {
            if (ManagerCoreInput.Instance != null)
            {
                return ManagerCoreInput.Instance.GetKey(type);
            }

            return false;
        }
        public static Vector2 GetAxis(this InputAction type)
        {
            if (ManagerCoreInput.Instance != null)
            {
                return ManagerCoreInput.Instance.GetAxis(type);
            }

            return Vector2.zero;
        }
    }
}
