using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public static class TaskUtility
    {
        public static Func<bool> WaitGame = WaitGameInternal;
        public static Func<bool> WaitResume = WaitResumeInternal;
        public static Func<bool> WaitPause = WaitPauseInternal;

        private static bool WaitGameInternal() => ManagerCoreGame.Instance != null;
        private static bool WaitResumeInternal() => ManagerCoreGame.Instance != null && ManagerCoreGame.Instance.GetGameState() == GameState.RESUME;
        private static bool WaitPauseInternal() => ManagerCoreGame.Instance != null && ManagerCoreGame.Instance.GetGameState() == GameState.PAUSE;

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
                Debug.LogWarning("WaitSeconds() seconds <= 0!");
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
                Debug.LogWarning("WaitSeconds() seconds <= 0!");
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

        public static TaskInstanceTweenOffsetLayout Offset(this LayoutGroup layoutGroup, RectOffset startValue, RectOffset targetValue, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenOffsetLayout tweenObject = new(layoutGroup, startValue, targetValue, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(tweenObject);

            return tweenObject;
        }
        public static TaskInstanceTweenOffsetRect Offset(this RectTransform rectTransform, Vector2 startValue, Vector2 targetValue, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenOffsetRect tweenObject = new(rectTransform, startValue, targetValue, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(tweenObject);

            return tweenObject;
        }
        public static TaskInstanceTweenOffsetRectX OffsetX(this RectTransform rectTransform, float startValue, float targetValue, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenOffsetRectX tweenObject = new(rectTransform, startValue, targetValue, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(tweenObject);

            return tweenObject;
        }
        public static TaskInstanceTweenOffsetRectY OffsetY(this RectTransform rectTransform, float startValue, float targetValue, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenOffsetRectY tweenObject = new(rectTransform, startValue, targetValue, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(tweenObject);

            return tweenObject;
        }
        public static TaskInstanceTweenFadeCanvas Fade(this CanvasGroup canvasGroup, float targetValue, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenFadeCanvas tweenObject = new(canvasGroup, targetValue, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(tweenObject);

            return tweenObject;
        }
        public static TaskInstanceTweenFadeImage Fade(this Image image, Color targetValue, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenFadeImage tweenObject = new(image, targetValue, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(tweenObject);

            return tweenObject;
        }
        public static TaskInstanceTweenScaleTransform Scale(this Transform transform, Vector3 targetValue, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenScaleTransform tweenObject = new(transform, targetValue, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(tweenObject);

            return tweenObject;
        }
    }
}