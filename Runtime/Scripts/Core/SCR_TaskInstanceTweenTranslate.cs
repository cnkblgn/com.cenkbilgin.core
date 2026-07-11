using System;
using UnityEngine;

namespace Core
{
    public sealed class TaskInstanceTweenTranslate : TaskInstanceTween
    {
        private readonly Transform transform;
        private Space space;
        private Vector3 start;
        private Vector3 target;

        public TaskInstanceTweenTranslate(Transform transform, Space space, Vector3 target, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(transform, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.transform = transform;
            this.target = target;
            this.space = space;
            this.start = Get();
        }

        protected override void OnFadeUpdate(float time) => Set(Vector3.Lerp(start, target, time));
        protected override void OnFadeComplete() => Set(target);

        private Vector3 Get() => space == Space.Self ? transform.localPosition : transform.position;
        private void Set(Vector3 target)
        {
            switch (space)
            {
                case Space.World: transform.position = target; break;
                case Space.Self: transform.localPosition = target; break;
                default: break;
            }
        }

        public void OverrideSpace(Space value) => space = value;
        public void OverrideStart(Vector3 value) => start = value;
        public void OverrideTarget(Vector3 value) => target = value;
    }
}
