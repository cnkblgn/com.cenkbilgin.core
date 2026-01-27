using System;
using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    public static class TaskUtility
    {
        public readonly static WaitGame _WaitGame = new();
        public readonly static WaitResume _WaitResume = new();
        public readonly static WaitPause _WaitPause = new();

        public readonly struct WaitGame : ITaskPredicate
        {
            public bool Evaluate() => ManagerCoreGame.Instance != null;
        }
        public readonly struct WaitResume : ITaskPredicate
        {
            public bool Evaluate() => ManagerCoreGame.Instance != null && ManagerCoreGame.Instance.GetGameState() == GameState.RESUME;
        }
        public readonly struct WaitPause : ITaskPredicate
        {
            public bool Evaluate() => ManagerCoreGame.Instance != null && ManagerCoreGame.Instance.GetGameState() == GameState.PAUSE;
        }
        public readonly struct WaitScene : ITaskPredicate
        {
            private readonly string name;
            public WaitScene(string name) => this.name = name;
            public bool Evaluate() => ManagerCoreGame.Instance != null && ManagerCoreGame.Instance.GetCurrentScene() == name;
        }
        public readonly struct SetTimescale : ITaskCallback
        {
            private readonly float scale;
            private readonly float duration;
            public SetTimescale(float scale, float duration)
            {
                this.scale = scale;
                this.duration = duration;
            }
            public void Invoke()
            {
                if (ManagerCoreGame.Instance == null)
                {
                    Debug.LogError("SetTimescale.Invoke() ManagerCoreGame.Instance == null");
                    return;
                }

                ManagerCoreGame.Instance.SetTimeScale(scale, duration);
            }
        }
        public readonly struct InvokePredicate : ITaskPredicate
        {
            private readonly Func<bool> callback;
            public InvokePredicate(Func<bool> callback) => this.callback = callback ?? throw new ArgumentNullException("InvokePredicate() callback == null");
            public bool Evaluate() => callback();
        }
        public readonly struct InvokeAction : ITaskCallback
        {
            private readonly Action callback;
            public InvokeAction(Action callback) => this.callback = callback ?? throw new ArgumentNullException("InvokeAction() callback == null");
            public void Invoke() => callback();
        }
        public readonly struct InvokeEvent : ITaskCallback
        {
            private readonly UnityEvent callback;
            public InvokeEvent(UnityEvent callback) => this.callback = callback;
            public void Invoke() => callback?.Invoke();
        }

        public static void WaitUntil<Predicate, OnStart, OnComplete>(this MonoBehaviour host, Predicate predicate, OnStart onStart, OnComplete onComplete) 
            where Predicate : struct, ITaskPredicate 
            where OnStart : struct, ITaskCallback
            where OnComplete : struct, ITaskCallback
        {
            onStart.Invoke();
            TaskSystem.TryCreate(new TaskInstanceWaitUntil<Predicate, OnComplete>(host, predicate, onComplete));
        }
        public static void WaitUntil(this MonoBehaviour host, InvokePredicate predicate, InvokeAction onStart, InvokeAction onComplete) => WaitUntil<InvokePredicate, InvokeAction, InvokeAction>(host, predicate, onStart, onComplete);

        public static TaskInstance WaitUntilExt<Predicate, OnStart, OnComplete>(this MonoBehaviour host, Predicate predicate, OnStart onStart, OnComplete onComplete) 
            where Predicate : struct, ITaskPredicate 
            where OnStart : struct, ITaskCallback
            where OnComplete : struct, ITaskCallback
        {
            onStart.Invoke();
            TaskInstanceWaitUntil<Predicate, OnComplete> instance = new(host, predicate, onComplete);
            TaskSystem.TryCreate(instance);
            return instance;
        }
        public static TaskInstance WaitUntilExt(this MonoBehaviour host, InvokePredicate predicate, InvokeAction onStart, InvokeAction onComplete) => WaitUntilExt<InvokePredicate, InvokeAction, InvokeAction>(host, predicate, onStart, onComplete);

        public static void WaitUntil<Predicate, OnComplete>(this MonoBehaviour host, Predicate predicate, OnComplete onComplete) 
            where Predicate : struct, ITaskPredicate 
            where OnComplete : struct, ITaskCallback
        {
            TaskSystem.TryCreate(new TaskInstanceWaitUntil<Predicate, OnComplete>(host, predicate, onComplete));
        }
        public static void WaitUntil(this MonoBehaviour host, InvokePredicate predicate, InvokeAction onComplete) => WaitUntil<InvokePredicate, InvokeAction>(host, predicate, onComplete);

        public static TaskInstance WaitUntilExt<Predicate, OnComplete>(this MonoBehaviour host, Predicate predicate, OnComplete onComplete) 
            where Predicate : struct, ITaskPredicate 
            where OnComplete : struct, ITaskCallback
        {
            TaskInstanceWaitUntil<Predicate, OnComplete> instance = new(host, predicate, onComplete);
            TaskSystem.TryCreate(instance);
            return instance;
        }
        public static TaskInstance WaitUntilExt(this MonoBehaviour host, InvokePredicate predicate, InvokeAction onComplete) => WaitUntilExt<InvokePredicate, InvokeAction>(host, predicate, onComplete);

        public static void WaitSeconds<OnComplete>(this MonoBehaviour host, float duration, OnComplete onComplete)
            where OnComplete : struct, ITaskCallback
        {
            if (duration <= 0)
            {
                onComplete.Invoke();
                Debug.LogWarning("WaitSeconds() seconds <= 0!");
                return;
            }

            TaskSystem.TryCreate(new TaskInstanceWaitSeconds<OnComplete>(host, onComplete, duration));
        }
        public static void WaitSeconds(this MonoBehaviour host, float duration, InvokeAction onComplete) => WaitSeconds<InvokeAction>(host, duration, onComplete);

        public static TaskInstance WaitSecondsExt<OnComplete>(this MonoBehaviour host, float duration, OnComplete onComplete)
            where OnComplete : struct, ITaskCallback
        {
            if (duration <= 0)
            {
                onComplete.Invoke();
                Debug.LogWarning("WaitSeconds() seconds <= 0!");
                return null;
            }

            TaskInstanceWaitSeconds<OnComplete> instance = new(host, onComplete, duration);

            TaskSystem.TryCreate(instance);

            return instance;
        }
        public static TaskInstance WaitSecondsExt(this MonoBehaviour host, float duration, InvokeAction onComplete) => WaitSecondsExt<InvokeAction>(host, duration, onComplete);

        public static void WaitSeconds<OnStart, OnComplete>(this MonoBehaviour host, float duration, OnStart onStart, OnComplete onComplete)
            where OnStart : struct, ITaskCallback
            where OnComplete : struct, ITaskCallback
        {
            onStart.Invoke();

            if (duration <= 0)
            {
                onComplete.Invoke();
                Debug.LogWarning("WaitSeconds() seconds <= 0!");
                return;
            }

            TaskSystem.TryCreate(new TaskInstanceWaitSeconds<OnComplete>(host, onComplete, duration));
        }
        public static void WaitSeconds(this MonoBehaviour host, float duration, InvokeAction onStart, InvokeAction onComplete) => WaitSeconds<InvokeAction, InvokeAction>(host, duration, onStart, onComplete);

        public static TaskInstance WaitSecondsExt<OnStart, OnComplete>(this MonoBehaviour host, float duration, OnStart onStart, OnComplete onComplete)
            where OnStart : struct, ITaskCallback
            where OnComplete : struct, ITaskCallback
        {
            onStart.Invoke();

            if (duration <= 0)
            {
                onComplete.Invoke();
                Debug.LogWarning("WaitSeconds() seconds <= 0!");
                return null;
            }

            TaskInstanceWaitSeconds<OnComplete> instance = new(host, onComplete, duration);

            TaskSystem.TryCreate(instance);

            return instance;
        }
        public static TaskInstance WaitSecondsExt(this MonoBehaviour host, float duration, InvokeAction onStart, InvokeAction onComplete) => WaitSecondsExt<InvokeAction, InvokeAction>(host, duration, onStart, onComplete);

        public static void WaitSecondsRealtime<OnComplete>(this MonoBehaviour host, float duration, OnComplete onComplete)
            where OnComplete : struct, ITaskCallback
        {
            if (duration <= 0)
            {
                onComplete.Invoke();
                Debug.LogWarning("WaitSeconds() seconds <= 0!");
                return;
            }

            TaskSystem.TryCreate(new TaskInstanceWaitSecondsRealtime<OnComplete>(host, onComplete, duration));
        }
        public static void WaitSecondsRealtime(this MonoBehaviour host, float duration, InvokeAction onComplete) => WaitSecondsRealtime<InvokeAction>(host, duration, onComplete);

        public static TaskInstance WaitSecondsRealtimeExt<OnComplete>(this MonoBehaviour host, float duration, OnComplete onComplete)
            where OnComplete : struct, ITaskCallback
        {
            if (duration <= 0)
            {
                onComplete.Invoke();
                Debug.LogWarning("WaitSeconds() seconds <= 0!");
                return null;
            }

            TaskInstanceWaitSecondsRealtime<OnComplete> instance = new(host, onComplete, duration);

            TaskSystem.TryCreate(instance);

            return instance;
        }
        public static TaskInstance WaitSecondsRealtimeExt(this MonoBehaviour host, float duration, InvokeAction onComplete) => WaitSecondsRealtimeExt<InvokeAction>(host, duration, onComplete);

        public static void WaitSecondsRealtime<OnStart, OnComplete>(this MonoBehaviour host, float duration, OnStart onStart, OnComplete onComplete)
            where OnStart : struct, ITaskCallback
            where OnComplete : struct, ITaskCallback
        {
            onStart.Invoke();

            if (duration <= 0)
            {
                onComplete.Invoke();
                Debug.LogWarning("WaitSeconds() seconds <= 0!");
                return;
            }

            TaskSystem.TryCreate(new TaskInstanceWaitSecondsRealtime<OnComplete>(host, onComplete, duration));
        }
        public static void WaitSecondsRealtime(this MonoBehaviour host, float duration, InvokeAction onStart, InvokeAction onComplete) => WaitSecondsRealtime<InvokeAction, InvokeAction>(host, duration, onStart, onComplete);

        public static TaskInstance WaitSecondsRealtimeExt<OnStart, OnComplete>(this MonoBehaviour host, float duration, OnStart onStart, OnComplete onComplete)
            where OnStart : struct, ITaskCallback
            where OnComplete : struct, ITaskCallback
        {
            onStart.Invoke();

            if (duration <= 0)
            {
                onComplete.Invoke();
                Debug.LogWarning("WaitSeconds() seconds <= 0!");
                return null;
            }

            TaskInstanceWaitSecondsRealtime<OnComplete> instance = new(host, onComplete, duration);

            TaskSystem.TryCreate(instance);

            return instance;
        }
        public static TaskInstance WaitSecondsRealtimeExt(this MonoBehaviour host, float duration, InvokeAction onStart, InvokeAction onComplete) => WaitSecondsRealtimeExt<InvokeAction, InvokeAction>(host, duration, onStart, onComplete);

        public static void WaitFrame<OnComplete>(this MonoBehaviour host, OnComplete onComplete)
            where OnComplete : struct, ITaskCallback
        {
            TaskSystem.TryCreate(new TaskInstanceWaitFrame<OnComplete>(host, onComplete));
        }
        public static void WaitFrame(this MonoBehaviour host, InvokeAction onComplete) => WaitFrame<InvokeAction>(host, onComplete);
    }
}