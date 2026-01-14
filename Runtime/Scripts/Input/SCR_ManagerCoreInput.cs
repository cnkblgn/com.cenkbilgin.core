using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Input
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class ManagerCoreInput : Manager<ManagerCoreInput>
    {
        public Vector3 PointerPosition { get; private set; } = Vector3.zero;
        public Vector2 PointerScroll { get; private set; } = Vector3.zero;
        public bool IsGamepadActive { get; private set; } = false;

        [Header("_")]
        public InputKey KeyAny = null;
        public InputKey KeyMenu = null;
        public InputKey KeyTab = null;
        public InputKey KeyConsole = null;
        public InputKey KeySubmit = null;
        public InputKey KeyContext = null;
        public InputKey KeyCancel = null;
        public InputKey KeyNavigateUp = null;
        public InputKey KeyNavigateDown = null;
        public InputKey KeyNavigateLeft = null;
        public InputKey KeyNavigateRight = null;
        public InputKey KeyPrimary = null;
        public InputKey KeySecondary = null;
        public InputKey KeyInteract = null;
        public InputKey KeyJump = null;
        public InputKey KeySprint = null;
        public InputKey KeyCrouch = null;

        [Header("_")]
        public InputAxis AxisLook = null;
        public InputAxis AxisMove = null;
        public InputAxis AxisScroll = null;

        private static readonly Dictionary<string, int> iconLookup = new()
        {
            // mouse
            { "leftButton", 1 },
            { "rightButton", 2 },
            { "middleButton", 3 },
            { "scroll", 3 },

            // keyboard
            { "space", 45 },
            { "enter", 38 },
            { "escape", 39 },
            { "leftShift", 44 },
            { "leftCtrl", 35 },
            { "leftAlt", 31 },
            { "r", 64 },
            { "f", 52 },
            { "g", 53 },
            { "q", 63 },
            { "e", 51 },
            { "v", 68 },
            { "b", 48 },
            { "tab", 46 },
            { "upArrow", 8 },
            { "downArrow", 5 },
            { "leftArrow", 6 },
            { "rightArrow", 7 },

            // gamepad
            { "buttonSouth", -1 },
            { "buttonEast", -1 }, 
            { "buttonWest", -1 }, 
            { "buttonNorth", -1 },
            { "leftStick", -1 },
            { "rightStick", -1 },
            { "leftShoulder", -1 },
            { "rightShoulder", -1 },
        };

        protected override void Awake()
        {
            base.Awake();

            KeyAny.Initialize();
            KeyMenu.Initialize();
            KeyTab.Initialize();
            KeyConsole.Initialize();
            KeySubmit.Initialize();
            KeyContext.Initialize();
            KeyCancel.Initialize();
            KeyNavigateUp.Initialize();
            KeyNavigateDown.Initialize();
            KeyNavigateLeft.Initialize();
            KeyNavigateRight.Initialize();
            KeyInteract.Initialize();
            KeyJump.Initialize();
            KeySprint.Initialize();
            KeyCrouch.Initialize();
            KeyPrimary.Initialize();
            KeySecondary.Initialize();

            AxisLook.Initialize();
            AxisMove.Initialize();
            AxisScroll.Initialize();

            Disable();
        }
        private void Update()
        {
            Gamepad cGamepad = Gamepad.current;
            if (cGamepad != null && (cGamepad.buttonSouth.wasPressedThisFrame || cGamepad.buttonNorth.wasPressedThisFrame || cGamepad.buttonEast.wasPressedThisFrame || cGamepad.buttonWest.wasPressedThisFrame || cGamepad.leftStick.ReadValue() != Vector2.zero || cGamepad.rightStick.ReadValue() != Vector2.zero))
            {
                IsGamepadActive = true;
            }

            Mouse cMouse = Mouse.current;
            if (cMouse != null && (cMouse.leftButton.wasPressedThisFrame || cMouse.rightButton.wasPressedThisFrame || cMouse.delta.ReadValue() != Vector2.zero || cMouse.scroll.ReadValue() != Vector2.zero))
            {
                IsGamepadActive = false;
            }

            Keyboard cKeyboard = Keyboard.current;
            if (cKeyboard != null && cKeyboard.anyKey.wasPressedThisFrame)
            {
                IsGamepadActive = false;
            }

            if (!IsGamepadActive)
            {
                PointerPosition = cMouse.position.ReadValue();
                PointerScroll = cMouse.scroll.ReadValue() * 0.1f;
            }

            KeyAny.Tick();
            KeyMenu.Tick();
            KeyTab.Tick();
            KeyConsole.Tick();
            KeySubmit.Tick();
            KeyContext.Tick();
            KeyCancel.Tick();
            KeyNavigateUp.Tick();
            KeyNavigateDown.Tick();
            KeyNavigateLeft.Tick();
            KeyNavigateRight.Tick();
            KeyInteract.Tick();
            KeyJump.Tick();
            KeySprint.Tick();
            KeyCrouch.Tick();
            KeyPrimary.Tick();
            KeySecondary.Tick();

            AxisLook.Tick();
            AxisMove.Tick();
            AxisScroll.Tick();
        }

        public void Enable()
        {
            KeyInteract.IsActive = true;
            KeyJump.IsActive = true;
            KeySprint.IsActive = true;
            KeyCrouch.IsActive = true;
            KeyPrimary.IsActive = true;
            KeySecondary.IsActive = true;

            AxisLook.IsActive = true;
            AxisMove.IsActive = true;
            AxisScroll.IsActive = true;
        }
        public void Disable()
        {
            KeyInteract.IsActive = false;
            KeyJump.IsActive = false;
            KeySprint.IsActive = false;
            KeyCrouch.IsActive = false;
            KeyPrimary.IsActive = false;
            KeySecondary.IsActive = false;

            AxisLook.IsActive = false;
            AxisMove.IsActive = false;
            AxisScroll.IsActive = false;
        }

        public int GetIcon(InputKey inputKeyData) => GetIcon(inputKeyData.Path);
        public int GetIcon(InputAxis inputAxisData) => GetIcon(inputAxisData.Path);
        private int GetIcon(string inputPath)
        {
            if (!iconLookup.TryGetValue(inputPath, out int icon))
            {
                return -1;
            }

            return icon;
        }
    }
}