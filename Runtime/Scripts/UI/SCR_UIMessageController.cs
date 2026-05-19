using System;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class UIMessageController : MonoBehaviour
    {
        public static event Action<string> OnMessageShow = null;
        public static event Action<string> OnMessageHide = null;

        [Header("_")]
        [SerializeField, Required] private CanvasGroup textCanvas = null;
        [SerializeField, Required] private UITextBox textBox = null;

        private Canvas thisCanvas = null;
        private TaskInstanceTweenFadeCanvas thisTween = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RESET()
        {
            OnMessageShow = null;
            OnMessageHide = null;
        }

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

            OnMessageShow?.Invoke(text);
        }
        public void Hide()
        {
            thisCanvas.Hide();
        }
        public void Clear()
        {
            OnMessageHide?.Invoke(textBox.Text);

            thisTween?.Stop();
            thisTween = null;

            textCanvas.Hide();
            textBox.Set(STRING_EMPTY);

            Hide();
        }
    }
}