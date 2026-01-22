using UnityEngine;

namespace Core
{
    using static CoreUtility;

    public static class TweenSystem
    {
        private const int MAX_TWEENS = 1024;
        private static readonly SwapBackArray<TweenInstance> ACTIVE_TWEENS = new(MAX_TWEENS);
        private static GameObject ACTIVE_TWEEN_OBJECT = null;
        private static Updater ACTIVE_TWEEN_UPDATER = null;
        private static bool isShuttingDown = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            isShuttingDown = false;

            ACTIVE_TWEEN_OBJECT = null;
            ACTIVE_TWEEN_UPDATER = null;
            ACTIVE_TWEENS.Clear();
        }
        private static bool IsValid()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                return false;
            }

            if (isShuttingDown)
            {
                return false;
            }

            if (ACTIVE_TWEEN_OBJECT == null)
            {
                ACTIVE_TWEEN_OBJECT = new GameObject("TWEEN_UPDATER");
                ACTIVE_TWEEN_UPDATER = ACTIVE_TWEEN_OBJECT.AddComponent<Updater>();
                UnityEngine.Object.DontDestroyOnLoad(ACTIVE_TWEEN_OBJECT);
            }

            return true;
        }
        private static void Update()
        {
            for (int i = ACTIVE_TWEENS.Count - 1; i >= 0; i--)
            {
                ACTIVE_TWEENS[i].Update();
            }

            int write = 0;
            for (int read = 0; read < ACTIVE_TWEENS.Count; read++)
            {
                TweenInstance tween = ACTIVE_TWEENS[read];

                if (tween != null && !tween.IsCompleted)
                {
                    ACTIVE_TWEENS[write] = tween;
                    write++;
                }
            }

            ACTIVE_TWEENS.Count = write;
        }
        private static void Clear()
        {
            for (int i = ACTIVE_TWEENS.Count - 1; i >= 0; i--)
            {
                ACTIVE_TWEENS[i] = null;
            }

            ACTIVE_TWEENS.Clear();
        }
        public static bool TryCreate(TweenInstance tweenInstance)
        {
            if (!IsValid())
            {
                return false;
            }

            ACTIVE_TWEENS.Add(tweenInstance);
            return true;
        }

        private class Updater : MonoBehaviour
        {
#pragma warning disable 0414
            [Header("_")]
            [SerializeField, ReadOnly] private int currentActiveTweens = -1;
            [SerializeField, ReadOnly] private int maximumActiveTweens = -1;
#pragma warning restore 0414

            private void Update()
            {
                currentActiveTweens = ACTIVE_TWEENS.Count;
                maximumActiveTweens = MAX_TWEENS;

                TweenSystem.Update();
            }
            private void OnDestroy()
            {
                isShuttingDown = true;
                Clear();
            }
        }
    }
}