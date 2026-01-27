using System;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;
    using static TaskUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public class UITransitionController : MonoBehaviour
    {
        private CanvasGroup thisCanvas = null;
        private TaskInstanceTweenFadeCanvas thisTween = null;

        private void Awake()
        {
            thisCanvas = GetComponent<CanvasGroup>();

            Hide();
        }
        /// <summary> 0 -> 1, fades to black </summary>
        public void FadeIn(float fadeTime, float waitTime, Action onStartAction, Action onFinishAction)
        {
            if (thisTween != null)
            {
                if (!thisTween.IsCompleted)
                {
                    onStartAction.Invoke();
                    this.WaitFrame(onFinishAction);
                    return;
                }
            }

            onStartAction.Invoke();
            thisCanvas.Show();
            thisCanvas.alpha = 0;

            thisTween = thisCanvas.Fade(1, fadeTime, waitTime, TweenType.UNSCALED, EaseType.EASE_OUT_SINE, () =>
            {
                thisTween.Stop();
                thisTween = null;
                onFinishAction.Invoke();
            });
        }
        /// <summary> 1 -> 0, fades to white </summary>
        public void FadeOut(float fadeTime, float waitTime, Action onStartAction, Action onFinishAction)
        {
            if (thisTween != null)
            {
                if (!thisTween.IsCompleted)
                {
                    onStartAction.Invoke();
                    this.WaitFrame(onFinishAction);
                    return;
                }
            }

            onStartAction.Invoke();
            thisCanvas.Show();
            thisCanvas.alpha = 1;

            thisTween = thisCanvas.Fade(0, fadeTime, waitTime, TweenType.UNSCALED, EaseType.EASE_IN_SINE, () =>
            {
                Hide();
                onFinishAction.Invoke();
            });
        }
        public void Hide()
        {
            thisCanvas.Hide();
            thisTween?.Stop();
            thisTween = null;
        }
    }
}