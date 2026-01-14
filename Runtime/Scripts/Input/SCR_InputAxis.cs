using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Input
{
    using static CoreUtility;

    [Serializable]
    public class InputAxis
    {
        public string Path { get; private set; } = STRING_NULL;
        public bool IsActive { get; set; } = true;
        public Vector2 Value
        {
            get
            {
                return value;
            }
        } private Vector2 value = Vector2.zero;

        [SerializeField, Required] private InputActionReference actionReferance = null;
        [SerializeField] private bool isNormalized = false;

        public InputAxis(InputAxis data)
        {
            if (data == null)
            {
                return;
            }

            actionReferance = data.actionReferance;
            isNormalized = data.isNormalized;
        }
        public void Initialize()
        {
            if (actionReferance == null)
            {
                LogError("InputAxis.Initialize() actionReferance == null");
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

            if (!IsActive)
            {
                value = Vector2.zero;
                return;
            }

            value = actionReferance.action.ReadValue<Vector2>();

            if (isNormalized)
            {
                value = value.normalized;
            }

            if (actionReferance.action.activeControl != null)
            {
                Path = actionReferance.action.activeControl.name;
            }
        }
    }
}