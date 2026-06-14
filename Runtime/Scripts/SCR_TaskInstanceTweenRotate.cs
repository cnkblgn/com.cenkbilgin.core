using System;
using UnityEngine;

namespace Core
{
    public sealed class TaskInstanceTweenRotate : TaskInstanceTween
    {
        private readonly Transform transform;
        private Space space;
        private Quaternion start;
        private Quaternion target;

        public TaskInstanceTweenRotate(Transform transform, Space space, Quaternion target, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(transform, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.transform = transform;
            this.space = space;
            this.target = target;
            this.start = Get();
        }

        protected override void OnFadeUpdate(float time) => Set(Quaternion.Slerp(start, target, time));
        protected override void OnFadeComplete() => Set(target);

        private Quaternion Get() => space == Space.Self ? transform.localRotation : transform.rotation;
        private void Set(Quaternion target)
        {
            switch (space)
            {
                case Space.World: transform.rotation = target; break;
                case Space.Self: transform.localRotation = target; break;
                default: break;
            }
        }

        public void OverrideSpace(Space value) => space = value;
        public void OverrideStart(Quaternion value) => start = value;
        public void OverrideTarget(Quaternion value) => target = value;
    }
}
