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
        private TaskInstanceTweenFadeCanvas thisTween = null;

        private void Awake() => thisCanvas = GetComponent<Canvas>();
        private void Start() { thisCanvas.Hide(); textCanvas.Hide(); }
        public void Show()
        {
            thisCanvas.Show();
        }
        public void Show(string text, float duration = 2.5f)
        {
            Show();

            thisTween?.Stop();

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
            thisCanvas.Hide();
        }
        public void Clear()
        {
            thisTween?.Stop();
            thisTween = null;

            textCanvas.Hide();
            textBox.Set(STRING_EMPTY);

            Hide();
        }
    }
}