using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;
    using static TaskUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UITransitionController : MonoBehaviour
    {
        private CanvasGroup thisCanvas = null;
        private TaskInstanceTweenFadeCanvas thisTween = null;

        private void Awake()
        {
            thisCanvas = GetComponent<CanvasGroup>();

            Hide();
        }

        /// <summary> 0 -> 1, fades to black </summary>
        public void FadeIn(UITransitionContext ctx)
        {
            if (thisTween != null)
            {
                if (!thisTween.IsCompleted)
                {
                    ctx.OnStart?.Invoke();

                    if (ctx.OnFinish != null)
                    {
                        this.WaitFrame(ctx.OnFinish);
                    }                   
                    return;
                }
            }

            ctx.OnStart?.Invoke();
            thisCanvas.Show();
            thisCanvas.alpha = 0;

            thisTween = thisCanvas.Fade(1, ctx.FadeTime, ctx.WaitTime, TweenType.UNSCALED, EaseType.EASE_OUT_SINE, () =>
            {
                thisTween.Stop();
                thisTween = null;
                ctx.OnFinish?.Invoke();
            });
        }
        /// <summary> 1 -> 0, fades to white </summary>
        public void FadeOut(UITransitionContext ctx)
        {
            if (thisTween != null)
            {
                if (!thisTween.IsCompleted)
                {
                    ctx.OnStart?.Invoke();

                    if (ctx.OnFinish != null)
                    {
                        this.WaitFrame(ctx.OnFinish);
                    }
                    return;
                }
            }

            ctx.OnStart?.Invoke();
            thisCanvas.Show();
            thisCanvas.alpha = 1;

            thisTween = thisCanvas.Fade(0, ctx.FadeTime, ctx.WaitTime, TweenType.UNSCALED, EaseType.EASE_IN_SINE, () =>
            {
                Hide();
                ctx.OnFinish?.Invoke();
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