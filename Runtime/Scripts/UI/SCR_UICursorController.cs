using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class UICursorController : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private RectTransform cursorTransform = null;
        [SerializeField, Required] private Image cursorImage = null;

        [Header("_")]
        [SerializeField] private UICursorData[] cursors = null;

        private readonly Dictionary<string, UICursorData> table = new();
        private Canvas thisCanvas = null;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            thisCanvas.Hide();

            foreach (UICursorData cursor in cursors)
            {
                table[cursor.ID] = cursor;
            }
        }
        private bool TryGetCursor(string id, out UICursorData cursor)
        {
            if (id == null)
            {
                throw new ArgumentNullException($"UICursorController.TryGetCursor() [{nameof(id)}]");
            }

            if (table.TryGetValue(id, out cursor))
            {
                return true;
            }

            Debug.LogWarning($"UICursorController.TryGetCursor() [{id}] is not defined");
            return false;
        }
        public void MoveCursor(Vector2 pointerPosition)
        {
            if (Cursor.lockState != CursorLockMode.Confined)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(thisCanvas.transform as RectTransform, pointerPosition, null, out Vector2 position);

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
            thisCanvas.Show();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
        public void HideCursor()
        {
            thisCanvas.Hide();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
