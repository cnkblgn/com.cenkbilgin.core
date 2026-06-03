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
        private bool hasMoved = false;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            thisCanvas.Hide();

            foreach (UICursorData cursor in cursors)
            {
                table[cursor.ID] = cursor;
            }
        }
#if UNITY_EDITOR
        private void LateUpdate()
        {
            if (!hasMoved)
            {
                Debug.LogWarning("Please update cursor position by calling MoveCursor()");
            }
        }
#endif

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
            if (Cursor.lockState != CursorLockMode.Confined)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(thisCanvas.transform as RectTransform, screenPosition, null, out Vector2 position);

            cursorTransform.localPosition = position;
            hasMoved = true;
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
