using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class UITooltipController : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private UITextBox textBox = null;

        private Canvas thisCanvas = null;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();

            Hide();
        }
        public void Move(Vector2 screenPosition)
        {
            if (thisCanvas == null)
            {
                return;
            }

            if (!thisCanvas.enabled)
            {
                return;
            }

            textBox.UpdateBounds(screenPosition);
        }
        public void Show(string text, Vector2 screenPosition)
        {
            thisCanvas.Show();
            textBox.Set(text);
            textBox.UpdateBounds(screenPosition);
        }
        public void Hide() => thisCanvas.Hide();
    }
}