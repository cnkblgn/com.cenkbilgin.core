using TMPro;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class UIMessageController : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private CanvasGroup textCanvas = null;
        [SerializeField, Required] private UITextBox textBox = null;

        private Canvas thisCanvas = null;
        private TweenFadeCanvas thisTween = null;

        private void Awake() => thisCanvas = GetComponent<Canvas>();
        private void Start() { thisCanvas.Show(); textCanvas.Hide(); }
        public void Show(string text, float duration = 2.5f)
        {
            thisTween?.Kill();

            if (duration > 0)
            {
                thisTween = textCanvas.Fade(duration, 1);
            }
            else
            {
                textCanvas.Show();
            }

            textBox.Set(text);
        }
        public void Hide()
        {
            thisTween?.Kill();
            thisTween = textCanvas.Fade(1, 0);
        }
    }
}