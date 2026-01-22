using System;
using UnityEngine;

namespace Core
{
    public class TweenInstanceScaleTransform : TweenInstance
    {
        protected override bool CanUpdate => thisTransform != null;

        private readonly Transform thisTransform = null;
        private readonly Vector3 startValue = Vector3.zero;
        private readonly Vector3 targetValue = Vector3.zero;

        public TweenInstanceScaleTransform(Transform rectTransform, Vector3 targetValue, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException("TweenInstanceScaleTransform() << " + nameof(rectTransform));
            }

            this.thisTransform = rectTransform;
            this.targetValue = targetValue;
            this.startValue = this.thisTransform.localScale;
        }

        protected override void OnFadeUpdate(float time) => thisTransform.localScale = Vector3.Lerp(startValue, targetValue, time);
        protected override void OnFadeComplete() => thisTransform.localScale = targetValue;
    }
}
