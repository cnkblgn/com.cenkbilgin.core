using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class TweenInstanceOffsetLayout : TweenInstance
    {
        protected override bool CanUpdate => thisLayout != null;

        private readonly LayoutGroup thisLayout = null;
        private readonly RectTransform thisRoot = null;
        private readonly RectOffset startValue = null;
        private readonly RectOffset targetValue = null;

        public TweenInstanceOffsetLayout(LayoutGroup layoutGroup, RectOffset startValue, RectOffset targetValue, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            if (layoutGroup == null)
            {
                throw new ArgumentNullException("TweenInstanceOffsetLayout() << " + nameof(layoutGroup));
            }

            thisLayout = layoutGroup;
            this.startValue = startValue;
            this.targetValue = targetValue;

            thisLayout.padding.left = startValue.left;
            thisLayout.padding.right = startValue.right;
            thisLayout.padding.top = startValue.top;
            thisLayout.padding.bottom = startValue.bottom;

            thisRoot = thisLayout.transform.parent.GetComponent<RectTransform>();
        }
        protected override void OnFadeUpdate(float time)
        {
            thisLayout.padding.left = (int)Mathf.Lerp(startValue.left, startValue.left, time);
            thisLayout.padding.right = (int)Mathf.Lerp(startValue.right, startValue.right, time);
            thisLayout.padding.top = (int)Mathf.Lerp(startValue.top, startValue.top, time);
            thisLayout.padding.bottom = (int)Mathf.Lerp(startValue.bottom, startValue.bottom, time);

            LayoutRebuilder.MarkLayoutForRebuild(thisRoot);
        }
        protected override void OnFadeComplete()
        {
            thisLayout.padding.left = targetValue.left;
            thisLayout.padding.right = targetValue.right;
            thisLayout.padding.top = targetValue.top;
            thisLayout.padding.bottom = targetValue.bottom;
        }
    }
}
