using System;
using UnityEngine;

namespace Core
{
    public static class TaskUtility
    {
        public static void WaitUntil(this MonoBehaviour _, Func<bool> predicate, Action onStart, Action onComplete)
        {
            onStart?.Invoke();
            TaskSystem.TryCreate(new TaskInstance(_, predicate, onComplete));
        }
        public static TaskInstance WaitUntilExt(this MonoBehaviour _, Func<bool> predicate, Action onStart, Action onComplete)
        {
            onStart?.Invoke();
            TaskInstance taskInstance = new(_, predicate, onComplete);
            TaskSystem.TryCreate(taskInstance);

            return taskInstance;
        }
        public static void WaitSeconds(this MonoBehaviour _, float seconds, bool isRealtime, Action onStart, Action onComplete)
        {
            if (seconds <= 0)
            {
                onStart?.Invoke();
                onComplete?.Invoke();
                Debug.LogWarning("WaitSeconds() seconds <= 0!");
                return;
            }

            onStart?.Invoke();
            TaskSystem.TryCreate(new(_, onComplete, seconds, isRealtime, false));
        }
        public static TaskInstance WaitSecondsExt(this MonoBehaviour _, float seconds, bool isRealtime, Action onStart, Action onComplete)
        {
            if (seconds <= 0)
            {
                onStart?.Invoke();
                onComplete?.Invoke();
                Debug.LogWarning("WaitSeconds() seconds <= 0!");
                return null;
            }

            onStart?.Invoke();
            TaskInstance taskInstance = new(_, onComplete, seconds, isRealtime, false);
            TaskSystem.TryCreate(taskInstance);
            return taskInstance;
        }
        public static void WaitFrame(this MonoBehaviour _, Action onStart, Action onComplete)
        {
            onStart?.Invoke();
            TaskSystem.TryCreate(new(_, onComplete, - 1, false, true));
        }
    }
}
