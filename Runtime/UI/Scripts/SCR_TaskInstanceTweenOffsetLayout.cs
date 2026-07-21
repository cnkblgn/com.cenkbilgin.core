using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public sealed class TaskInstanceTweenOffsetLayout : TaskInstanceTween
    {
        private readonly LayoutGroup layout = null;
        private readonly RectTransform root = null;
        private RectOffset start = null;
        private RectOffset target = null;

        public TaskInstanceTweenOffsetLayout(LayoutGroup layout, RectOffset start, RectOffset target, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(layout, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.layout = layout;
            this.start = start;
            this.target = target;

            this.layout.padding.left = start.left;
            this.layout.padding.right = start.right;
            this.layout.padding.top = start.top;
            this.layout.padding.bottom = start.bottom;

            root = this.layout.transform.parent.GetComponent<RectTransform>();
        }
        protected override void OnFadeUpdate(float time)
        {
            layout.padding.left = (int)Mathf.Lerp(start.left, start.left, time);
            layout.padding.right = (int)Mathf.Lerp(start.right, start.right, time);
            layout.padding.top = (int)Mathf.Lerp(start.top, start.top, time);
            layout.padding.bottom = (int)Mathf.Lerp(start.bottom, start.bottom, time);

            LayoutRebuilder.MarkLayoutForRebuild(root);
        }
        protected override void OnFadeComplete()
        {
            layout.padding.left = target.left;
            layout.padding.right = target.right;
            layout.padding.top = target.top;
            layout.padding.bottom = target.bottom;
        }

        public void OverrideStart(RectOffset value) => start = value;
        public void OverrideTarget(RectOffset value) => target = value;
    }
}
