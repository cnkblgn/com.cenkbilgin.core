using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Input
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInput))]
    public class ManagerCoreInput : Manager<ManagerCoreInput>
    {
        public Vector3 PointerPosition { get; private set; } = Vector3.zero;
        public Vector2 PointerScroll { get; private set; } = Vector3.zero;
        public bool IsGamepadActive { get; private set; } = false;

        private PlayerInput thisInput = null;
        private InputActionAsset thisActions = null;
        private InputActionRebindingExtensions.RebindingOperation actionOperation = null;
        private readonly Dictionary<string, InputAction> actionLookup = new();
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

            thisInput = GetComponent<PlayerInput>();           
            thisInput.onControlsChanged += OnControlsChanged;
            thisActions = thisInput.actions;

            actionLookup.Clear();

            foreach (var map in thisActions.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    actionLookup[$"{map.name}.{action.name}"] = action;
                }
            }

            Import();
            GetMap("Global").Enable();
            SwitchMap("Gameplay");
        }
        private void Update()
        {
            if (!IsGamepadActive && Mouse.current != null)
            {
                PointerPosition = Mouse.current.position.ReadValue();
                PointerScroll = Mouse.current.scroll.ReadValue() * 0.1f;
            }
        }
        private void OnDestroy() => thisInput.onControlsChanged -= OnControlsChanged;

        private void OnControlsChanged(PlayerInput input) => IsGamepadActive = input.currentControlScheme == "Gamepad";

        public void Enable() { thisInput.ActivateInput(); GetMap("Global").Enable(); }
        public void Disable() => thisInput.DeactivateInput();
        public void SwitchMap(string name)
        {
            if (thisInput.currentActionMap.name == name)
            {
                return;
            }

            thisInput.SwitchCurrentActionMap(name);
        }
        private InputActionMap GetMap(string name) => thisActions.FindActionMap(name, true);
        private InputAction GetAction(InputActionType type) => GetAction(type.Value);
        private InputAction GetAction(string path)
        {
            if (!actionLookup.TryGetValue(path, out var action))
            {
                Debug.LogError($"ManagerCoreInput.GetAction() [{path}] not found!");
                return null;
            }

            return action;
        }
        public bool GetButton(InputActionType type) => GetAction(type)?.IsPressed() == true;
        public bool GetButtonDown(InputActionType type) => GetAction(type)?.WasPressedThisFrame() == true;
        public bool GetButtonUp(InputActionType type) => GetAction(type)?.WasReleasedThisFrame() == true;
        public Vector2 GetVector(InputActionType type) => GetAction(type)?.ReadValue<Vector2>() ?? Vector2.zero;
        public int GetIcon(InputActionType type) => GetIcon(type.Value);
        public int GetIcon(string path)
        {
            InputAction action = GetAction(path);

            if (action == null)
            {
                return -1;
            }

            if (action.controls.Count <= 0)
            {
                return -1;
            }

            if (!iconLookup.TryGetValue(action.controls[0].name, out int icon))
            {
                return -1;
            }

            return icon;
        }
        public string GetDisplay(InputActionType type, int bindingIndex) => GetDisplay(type.Value, bindingIndex);
        public string GetDisplay(string path, int bindingIndex)
        {
            InputAction action = GetAction(path);

            if (action == null)
            {
                return STRING_NULL;
            }

            return action.GetBindingDisplayString(bindingIndex);
        }
        public void StartRebind(InputActionType type, int bindingIndex, Action onStart, Action onComplete, Action onCancel) => StartRebind(type.Value, bindingIndex, onStart, onComplete, onCancel);
        public void StartRebind(string path, int bindingIndex, Action onStart, Action onComplete, Action onCancel)
        {
            InputAction action = GetAction(path);

            if (action == null)
            {
                Debug.LogWarning("ManagerCoreInput.StartRebind() action == null");
                return;
            }

            onStart?.Invoke();
            action.Disable();

            if (actionOperation != null)
            {
                actionOperation.Dispose();
                actionOperation = null;
            }

            action.RemoveBindingOverride(bindingIndex);

            actionOperation = action.PerformInteractiveRebinding(bindingIndex)
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(_ =>
            {
                actionOperation.Dispose();
                action.Enable();
                onComplete?.Invoke();
            })
            .OnCancel(_ =>
            {
                actionOperation.Dispose();
                action.Enable();
                onCancel?.Invoke();
            })
            .Start();
        }
        public void RevertRebind(InputActionType type, int bindingIndex) => RevertRebind(type.Value, bindingIndex);
        public void RevertRebind(string path, int bindingIndex)
        {
            InputAction action = GetAction(path);

            if (action == null)
            {
                Debug.LogWarning("ManagerCoreInput.RevertRebind() action == null");
                return;
            }

            action.RemoveBindingOverride(bindingIndex);
        }
        public void Export()
        {
            if (thisActions == null)
            {
                return;
            }

            string json = thisActions.SaveBindingOverridesAsJson();
            string path = Path.Combine(Application.persistentDataPath, "inputs.json");
            try { File.WriteAllText(path, json); }
            catch (Exception ex) { Debug.LogError($"ManagerCoreInput.Export() failed [{ex}]"); }
        }
        private void Import()
        {
            if (thisActions == null)
            {
                return;
            }

            string path = Path.Combine(Application.persistentDataPath, "inputs.json");

            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);

                    if (!string.IsNullOrEmpty(json))
                    {
                        thisActions.LoadBindingOverridesFromJson(json);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"ManagerCoreInput.Export() failed [{ex}]");
                }
            }
            else
            {
                thisActions.RemoveAllBindingOverrides();
            }
        }
    }
}