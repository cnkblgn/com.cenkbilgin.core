using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public class UICrosshairController : MonoBehaviour
    {
#pragma warning disable 0414
        [Header("_")]
        [SerializeField, Required] private Image crosshairImage = null;
#pragma warning restore 0414

        private CanvasGroup thisCanvas = null;

        private void Awake()
        {
            thisCanvas = GetComponent<CanvasGroup>();
            thisCanvas.Hide();
        }
        public void Show() => thisCanvas.Show(false, false);
        public void Hide() => thisCanvas.Hide();
    }
}