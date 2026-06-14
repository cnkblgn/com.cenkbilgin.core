using System;
using UnityEngine;

namespace Core
{
    public sealed class TaskInstanceTweenScale : TaskInstanceTween
    {
        private readonly Transform transform;
        private Vector3 start;
        private Vector3 target;

        public TaskInstanceTweenScale(Transform transform, Vector3 target, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(transform, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.transform = transform;
            this.target = target;
            this.start = this.transform.localScale;
        }

        protected override void OnFadeUpdate(float time) => transform.localScale = Vector3.Lerp(start, target, time);
        protected override void OnFadeComplete() => transform.localScale = target;

        public void OverrideStart(Vector3 value) => start = value;
        public void OverrideTarget(Vector3 value) => target = value;
    }
}
