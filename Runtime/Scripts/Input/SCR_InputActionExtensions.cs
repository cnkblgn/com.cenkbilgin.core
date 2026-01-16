using UnityEngine;

namespace Core.Input
{
    using static CoreUtility;

    public static class InputActionExtensions
    {
        public static bool GetKeyUp(this InputActionType type)
        {
            if (ManagerCoreInput.Instance != null)
            {
                return ManagerCoreInput.Instance.GetKeyUp(type);
            }

            return false;
        }
        public static bool GetKeyDown(this InputActionType type)
        {
            if (ManagerCoreInput.Instance != null)
            {
                return ManagerCoreInput.Instance.GetKeyDown(type);
            }

            return false;
        }
        public static bool GetKey(this InputActionType type)
        {
            if (ManagerCoreInput.Instance != null)
            {
                return ManagerCoreInput.Instance.GetKey(type);
            }

            return false;
        }
        public static Vector2 GetAxis(this InputActionType type)
        {
            if (ManagerCoreInput.Instance != null)
            {
                return ManagerCoreInput.Instance.GetAxis(type);
            }

            return Vector2.zero;
        }
    }
}
