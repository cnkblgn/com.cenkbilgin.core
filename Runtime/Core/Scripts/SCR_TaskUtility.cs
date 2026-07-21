using System;
using UnityEngine;

namespace Core
{
    public static class TaskUtility
    {
        public readonly static Func<bool> WaitGame = WaitGameInternal;
        public readonly static Func<bool> WaitResume = WaitResumeInternal;
        public readonly static Func<bool> WaitPause = WaitPauseInternal;

        private static bool WaitGameInternal() => ManagerGame.HasInstance;
        private static bool WaitResumeInternal() => ManagerGame.HasInstance && ManagerGame.Instance.GetGameState() == GameState.RESUME;
        private static bool WaitPauseInternal() => ManagerGame.HasInstance && ManagerGame.Instance.GetGameState() == GameState.PAUSE;

        public static void Schedule(this TaskInstanceWaitFrame task)
        {
            task.Reset();

            TaskSystem.TryCreate(task);
        }
        public static void Schedule(this TaskInstanceWaitSeconds task, float duration)
        {
            task.OverrideDuration(duration);

            task.Reset();

            TaskSystem.TryCreate(task);
        }
        public static void Schedule(this TaskInstanceWaitSecondsRealtime task, float duration)
        {
            task.OverrideDuration(duration);

            task.Reset();

            TaskSystem.TryCreate(task);
        }

        public static void Schedule(this TaskInstanceTweenTranslate task, Space space, Vector3 start, Vector3 target)
        {
            task.Reset();

            task.OverrideSpace(space);
            task.OverrideStart(start);
            task.OverrideTarget(target);

            TaskSystem.TryCreate(task);
        }
        public static void Schedule(this TaskInstanceTweenRotate task, Space space, Quaternion start, Quaternion target)
        {
            task.Reset();

            task.OverrideSpace(space);
            task.OverrideStart(start);
            task.OverrideTarget(target);

            TaskSystem.TryCreate(task);
        }
        public static void Schedule(this TaskInstanceTweenScale task, Vector3 start, Vector3 target)
        {
            task.Reset();

            task.OverrideStart(start);
            task.OverrideTarget(target);

            TaskSystem.TryCreate(task);
        }

        public static void WaitUntil(this MonoBehaviour host, Func<bool> predicate, Action onStart, Action onComplete)
        {
            onStart?.Invoke();

            TaskSystem.TryCreate(new TaskInstanceWaitUntil(host, predicate, onComplete));
        }
        public static TaskInstance WaitUntilExt(this MonoBehaviour host, Func<bool> predicate, Action onStart, Action onComplete)
        {
            onStart?.Invoke();

            TaskInstanceWaitUntil instance = new(host, predicate, onComplete);

            TaskSystem.TryCreate(instance);

            return instance;
        }
        public static void WaitUntil(this MonoBehaviour host, Func<bool> predicate, Action onComplete)
        {
            TaskSystem.TryCreate(new TaskInstanceWaitUntil(host, predicate, onComplete));
        }
        public static TaskInstance WaitUntilExt(this MonoBehaviour host, Func<bool> predicate, Action onComplete)
        {
            TaskInstanceWaitUntil instance = new(host, predicate, onComplete);

            TaskSystem.TryCreate(instance);

            return instance;
        }
        public static void WaitSeconds(this MonoBehaviour host, float duration, Action onStart, Action onComplete)
        {
            onStart?.Invoke();

            if (duration <= 0)
            {
                onComplete?.Invoke();
                return;
            }

            TaskSystem.TryCreate(new TaskInstanceWaitSeconds(host, duration, onComplete));
        }
        public static TaskInstance WaitSecondsExt(this MonoBehaviour host, float duration, Action onStart, Action onComplete)
        {
            onStart?.Invoke();

            if (duration <= 0)
            {
                onComplete?.Invoke();
                return null;
            }

            TaskInstanceWaitSeconds instance = new(host, duration, onComplete);

            TaskSystem.TryCreate(instance);

            return instance;
        }
        public static void WaitSeconds(this MonoBehaviour host, float duration, Action onComplete)
        {
            if (duration <= 0)
            {
                onComplete?.Invoke();
                return;
            }

            TaskSystem.TryCreate(new TaskInstanceWaitSeconds(host, duration, onComplete));
        }
        public static TaskInstance WaitSecondsExt(this MonoBehaviour host, float duration, Action onComplete)
        {
            if (duration <= 0)
            {
                onComplete?.Invoke();
                return null;
            }

            TaskInstanceWaitSeconds instance = new(host, duration, onComplete);

            TaskSystem.TryCreate(instance);

            return instance;
        }
        public static void WaitSecondsRealtime(this MonoBehaviour host, float duration, Action onStart, Action onComplete)
        {
            onStart?.Invoke();

            if (duration <= 0)
            {
                onComplete?.Invoke();
                return;
            }

            TaskSystem.TryCreate(new TaskInstanceWaitSecondsRealtime(host, duration, onComplete));
        }
        public static TaskInstance WaitSecondsRealtimeExt(this MonoBehaviour host, float duration, Action onStart, Action onComplete)
        {
            onStart?.Invoke();

            if (duration <= 0)
            {
                onComplete?.Invoke();
                return null;
            }

            TaskInstanceWaitSecondsRealtime instance = new(host, duration, onComplete);

            TaskSystem.TryCreate(instance);

            return instance;
        }
        public static void WaitSecondsRealtime(this MonoBehaviour host, float duration, Action onComplete)
        {
            if (duration <= 0)
            {
                onComplete?.Invoke();
                return;
            }

            TaskSystem.TryCreate(new TaskInstanceWaitSecondsRealtime(host, duration, onComplete));
        }
        public static TaskInstance WaitSecondsExtRealtime(this MonoBehaviour host, float duration, Action onComplete)
        {
            if (duration <= 0)
            {
                onComplete?.Invoke();
                return null;
            }

            TaskInstanceWaitSecondsRealtime instance = new(host, duration, onComplete);

            TaskSystem.TryCreate(instance);

            return instance;
        }
        public static void WaitFrame(this MonoBehaviour host, Action onComplete)
        {
            TaskSystem.TryCreate(new TaskInstanceWaitFrame(host, onComplete));
        }
        public static void WaitFrame(this MonoBehaviour host, Action onStart, Action onComplete)
        {
            onStart?.Invoke();

            TaskSystem.TryCreate(new TaskInstanceWaitFrame(host, onComplete));
        }

        public static TaskInstanceTweenTranslate Translate(this Transform transform, Space space, Vector3 target, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.SCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenTranslate obj = new(transform, space, target, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);

            TaskSystem.TryCreate(obj);

            return obj;
        }
        public static TaskInstanceTweenRotate Rotate(this Transform transform, Space space, Quaternion target, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.SCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenRotate obj = new(transform, space, target, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);

            TaskSystem.TryCreate(obj);

            return obj;
        }
        public static TaskInstanceTweenScale Scale(this Transform transform, Vector3 target, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.SCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenScale obj = new(transform, target, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);

            TaskSystem.TryCreate(obj);

            return obj;
        }
    }
}