using System;

namespace Core.Graphics
{
    public static class GraphicsUtility
    {
        public static void Schedule(this TaskInstanceTweenFadeDecal task, float start, float target)
        {
            task.Reset();

            task.OverrideStart(start);
            task.OverrideTarget(target);

            TaskSystem.TryCreate(task);
        }

        public static TaskInstanceTweenFadeDecal Fade(this DecalEmitter decal, float target, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenFadeDecal obj = new(decal, target, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(obj);

            return obj;
        }
    }
}