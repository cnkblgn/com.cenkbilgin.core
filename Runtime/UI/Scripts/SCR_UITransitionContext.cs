using System;

namespace Core.UI
{
    public readonly struct UITransitionContext
    {
        public readonly float FadeTime;
        public readonly float WaitTime;
        public readonly Action OnStart;
        public readonly Action OnFinish;

        public UITransitionContext(float fadeTime, float waitTime) : this(fadeTime, waitTime, null, null) { }
        public UITransitionContext(float fadeTime, float waitTime, Action onStart, Action onFinish)
        {
            FadeTime = fadeTime;
            WaitTime = waitTime;
            OnStart = onStart;
            OnFinish = onFinish;
        }
    }
}