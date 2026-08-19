using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    internal sealed class UICursorController : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private RectTransform cursorTransform;
        [SerializeField, Required] private Image cursorImage;

        [Header("_")]
        [SerializeField] private UICursorData[] cursors;

        private readonly Dictionary<string, UICursorData> table = new();
        private Canvas canvas;
        private bool hasFocus;

        private void Awake()
        {
            hasFocus = Application.isFocused;

            canvas = GetComponent<Canvas>();
            canvas.Hide();

            foreach (UICursorData cursor in cursors)
            {
                table[cursor.ID] = cursor;
            }
        }
        private void OnApplicationFocus(bool focus) => hasFocus = focus;

        private static bool IsValid(Vector2 value) => float.IsFinite(value.x) && float.IsFinite(value.y);

        private bool TryGetCursor(string id, out UICursorData cursor)
        {
            if (id == null)
            {
                throw new ArgumentNullException($"id == null [{nameof(id)}]");
            }

            if (table.TryGetValue(id, out cursor))
            {
                return true;
            }

            Debug.LogWarning($"[{id}] is not defined");
            return false;
        }

        public void MoveCursor(Vector2 screenPosition)
        {
            if (!hasFocus)
            {
                return;
            }

            if (Cursor.lockState != CursorLockMode.Confined)
            {
                return;
            }

            if (!IsValid(screenPosition))
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPosition, null, out Vector2 position);

            if (!IsValid(position))
            {
                return;
            }

            cursorTransform.localPosition = position;
        }
        public void SetCursor(string id)
        {
            if (TryGetCursor(id, out UICursorData cursor))
            {
                cursorImage.sprite = cursor.Icon;
            }
        }
        public void ShowCursor()
        {
            canvas.Show();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
        public void HideCursor()
        {
            canvas.Hide();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
