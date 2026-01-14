using UnityEngine;
using TMPro;
using Core.Input;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public class UITooltipController : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private UITextBox textBox = null;

        private CanvasGroup thisCanvas = null;

        private void Awake()
        {
            thisCanvas = GetComponent<CanvasGroup>();

            Hide();
        }
        private void Update()
        {
            if (thisCanvas == null)
            {
                return;
            }

            if (!thisCanvas.enabled)
            {
                return;
            }

            textBox.UpdateBounds(ManagerCoreInput.Instance.PointerPosition);
        }
        public void Show(string text)
        {
            thisCanvas.Show(false, false);
            textBox.Set(text);
            textBox.UpdateBounds(ManagerCoreInput.Instance.PointerPosition);
        }
        public void Hide() => thisCanvas.Hide();
    }
}