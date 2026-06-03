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
    public sealed class ManagerCoreInput : Manager<ManagerCoreInput>
    {
        public Vector3 PointerPosition { get; private set; } = Vector3.zero;
        public Vector2 PointerScroll { get; private set; } = Vector3.zero;
        public bool IsGamepadActive { get; private set; } = false;

        private PlayerInput thisInput = null;
        private InputActionAsset thisActions = null;
        private InputActionRebindingExtensions.RebindingOperation actionOperation = null;
        private readonly Dictionary<string, UnityEngine.InputSystem.InputAction> actionLookup = new();
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
        private static readonly InputMap defaultMap = new("Global");

        protected override void Awake()
        {
            base.Awake();

            thisInput = GetComponent<PlayerInput>();           
            thisInput.neverAutoSwitchControlSchemes = true;
            thisInput.defaultActionMap = defaultMap.name;
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
            EnableMap(defaultMap);
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

        public void Enable() => thisInput.ActivateInput();
        public void Disable() => thisInput.DeactivateInput();

        private void SwitchMap(string name)
        {
            EnableMap(defaultMap);

            if (thisInput.currentActionMap != null && thisInput.currentActionMap.name == name)
            {
                return;
            }

            thisInput.SwitchCurrentActionMap(name);
        }
        public void SwitchMap(InputMap map) => SwitchMap(map.name);
        public void EnableMap(InputMap map) => GetMap(map.name)?.Enable();
        public void DisableMap(InputMap map) => GetMap(map.name)?.Disable();
        private InputActionMap GetMap(string name) => thisActions.FindActionMap(name);
        public string GetMap() => thisInput.currentActionMap != null ? thisInput.currentActionMap.name : STRING_NULL;
        private UnityEngine.InputSystem.InputAction GetAction(InputAction type) => GetAction(type.path);
        private UnityEngine.InputSystem.InputAction GetAction(string path)
        {
            if (!actionLookup.TryGetValue(path, out var action))
            {
                Debug.LogError($"[{path}] not found!");
                return null;
            }

            return action;
        }
        public bool GetKey(InputAction type) => GetAction(type)?.IsPressed() == true;
        public bool GetKeyDown(InputAction type) => GetAction(type)?.WasPressedThisFrame() == true;
        public bool GetKeyUp(InputAction type) => GetAction(type)?.WasReleasedThisFrame() == true;
        public Vector2 GetAxis(InputAction type) => GetAction(type)?.ReadValue<Vector2>() ?? Vector2.zero;
        public int GetIcon(InputAction type) => GetIcon(type.path);
        public int GetIcon(string path)
        {
            UnityEngine.InputSystem.InputAction action = GetAction(path);

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
        public string GetDisplay(InputAction type, int bindingIndex) => GetDisplay(type.path, bindingIndex);
        public string GetDisplay(string path, int bindingIndex)
        {
            UnityEngine.InputSystem.InputAction action = GetAction(path);

            if (action == null)
            {
                return STRING_NULL;
            }

            return action.GetBindingDisplayString(bindingIndex);
        }
        public void StartRebind(InputAction type, int bindingIndex, Action onStart, Action onComplete, Action onCancel) => StartRebind(type.path, bindingIndex, onStart, onComplete, onCancel);
        public void StartRebind(string path, int bindingIndex, Action onStart, Action onComplete, Action onCancel)
        {
            UnityEngine.InputSystem.InputAction action = GetAction(path);

            if (action == null)
            {
                Debug.LogWarning("action == null");
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
        public void RevertRebind(InputAction type, int bindingIndex) => RevertRebind(type.path, bindingIndex);
        public void RevertRebind(string path, int bindingIndex)
        {
            UnityEngine.InputSystem.InputAction action = GetAction(path);

            if (action == null)
            {
                Debug.LogWarning("action == null");
                return;
            }

            action.RemoveBindingOverride(bindingIndex);
        }

        public void Export(string name = "inputs.json")
        {
            if (thisActions == null)
            {
                return;
            }

            string json = thisActions.SaveBindingOverridesAsJson();
            string path = Path.Combine(Application.persistentDataPath, name);

            try 
            { 
                File.WriteAllText(path, json);
            }
            catch (Exception e) 
            { 
                Debug.LogError($"failed [{e}]"); 
            }
        }
        private void Import(string name = "inputs.json")
        {
            if (thisActions == null)
            {
                return;
            }

            string path = Path.Combine(Application.persistentDataPath, name);

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
                    Debug.LogError($"failed [{ex}]");
                }
            }
            else
            {
                thisActions.RemoveAllBindingOverrides();
            }
        }
    }
}