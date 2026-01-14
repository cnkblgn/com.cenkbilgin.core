using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Core.Input
{
    using static CoreUtility;

    [Serializable]
    public class InputKey
    {
        public string Path { get; private set; } = STRING_NULL;
        public bool IsActive { get; set; } = true;
        public bool IsPressing { get; private set; } = false;
        public bool IsPressedDown { get; private set; } = false;
        public bool IsPressedUp { get; private set; } = false;

        [SerializeField, Required] private InputActionReference actionReferance = null;

        public void Initialize()
        {
            if (actionReferance == null)
            {
                LogError("InputKey.Initialize() actionReferance == null");
                return;
            }

            actionReferance.action.Enable();

            if (actionReferance.action.controls.Count > 0)
            {
                Path = actionReferance.action.controls[0].name;
            }
        }
        public void Tick()
        {
            if (actionReferance == null)
            {
                return;
            }

            // Used for actions triggered on key release (e.g., firing a charged shot or toggling).
            bool keyUp = actionReferance.action.WasReleasedThisFrame();
            // Used for one-time triggers (e.g., shooting a bullet or jumping).
            bool keyDown = actionReferance.action.WasPressedThisFrame();
            // Used for actions that occur while the key is held down (e.g., charging a weapon or running).
            bool keyPress = actionReferance.action.IsPressed();

            IsPressing = IsActive && keyPress;
            IsPressedUp = IsActive && keyUp;
            IsPressedDown = IsActive && keyDown;

            if (actionReferance.action.activeControl != null)
            {
                Path = actionReferance.action.activeControl.name;
            }
        }
    }
}
