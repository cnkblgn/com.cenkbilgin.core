using UnityEngine;

namespace Core
{
    using static CoreUtility;

    public static class TaskSystem
    {
        private const int MAX_TASKS = 2048;
        private static readonly SwapBackArray<TaskInstance> activeTasks = new(MAX_TASKS);
        private static GameObject activeObject = null;
        private static Updater activeUpdater = null;
        private static bool isShuttingDown = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            isShuttingDown = false;

            activeObject = null;
            activeUpdater = null;
            activeTasks.Clear();
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

            if (activeObject == null)
            {
                activeObject = new GameObject("[Task Updater]") { hideFlags = HideFlags.NotEditable };
                activeUpdater = activeObject.AddComponent<Updater>();
                UnityEngine.Object.DontDestroyOnLoad(activeObject);
            }

            return true;
        }
        private static void Update()
        {
            int write = 0;
            for (int read = 0; read < activeTasks.Count; read++)
            {
                TaskInstance task = activeTasks[read];

                if (!task.IsCompleted)
                {
                    task.Update();
                    activeTasks[write++] = task;
                }
            }

            activeTasks.Truncate(write);
        }
        private static void Clear()
        {
            activeTasks.Clear();
        }
        public static void Insert(TaskInstance taskInstance)
        {
            if (!IsValid())
            {
                return;
            }

            activeTasks.Add(taskInstance);
        }

        private sealed class Updater : MonoBehaviour
        {
            [Header("_")]
#pragma warning disable CS0414
            [SerializeField, ReadOnly] private int currentActiveTasks = -1;
            [SerializeField, ReadOnly] private int maximumActiveTasks = -1;
#pragma warning restore CS0414

            private void Update()
            {
                currentActiveTasks = activeTasks.Count;
                maximumActiveTasks = MAX_TASKS;

                TaskSystem.Update();
            }
            private void OnDestroy()
            {
                isShuttingDown = true;
                Clear();
            }
        }
    }
}