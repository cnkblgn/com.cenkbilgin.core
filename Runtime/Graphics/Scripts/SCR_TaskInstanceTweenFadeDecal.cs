using System;
using UnityEngine;

namespace Core.Graphics
{
    using static CoreUtility;

    public sealed class TaskInstanceTweenFadeDecal : TaskInstanceTween
    {
        private readonly DecalEmitter decal = null;
        private float start;
        private float target;

        public TaskInstanceTweenFadeDecal(DecalEmitter decal, float target, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(decal, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.decal = decal;
            this.target = target;
            this.start = this.decal.GetAlpha();
        }

        protected override void OnFadeUpdate(float time) => decal.SetAlpha(Mathf.Lerp(start, target, time));
        protected override void OnFadeComplete() => decal.SetAlpha(target);

        public void OverrideStart(float value) => start = value;
        public void OverrideTarget(float value) => target = value;
    }
}
