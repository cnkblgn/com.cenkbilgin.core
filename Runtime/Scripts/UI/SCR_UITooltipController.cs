using System;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class UITooltipController : MonoBehaviour
    {
        public static event Action<string> OnTooltipShow = null;
        public static event Action<string> OnTooltipHide = null;

        [Header("_")]
        [SerializeField, Required] private UITextBox textBox = null;

        private Canvas thisCanvas = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RESET()
        {
            OnTooltipShow = null;
            OnTooltipHide = null;
        }

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

            OnTooltipShow?.Invoke(text);
        }
        public void Hide()
        {
            thisCanvas.Hide();

            OnTooltipHide?.Invoke(textBox.Text);
        }
    }
}