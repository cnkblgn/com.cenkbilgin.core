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

        private Canvas thisCanvas = null;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            thisCanvas.Hide();
        }
        private UICursorData GetCursor(UICursorType type)
        {
            foreach (UICursorData cursor in cursors)
            {
                if (cursor.Type == type)
                {
                    return cursor;
                }
            }

            return null;
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
        public void SetCursor(UICursorType type = UICursorType.DEFAULT)
        {
            UICursorData cursorData = GetCursor(type);

            if (cursorData != null)
            {
                cursorImage.sprite = cursorData.Icon;
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
