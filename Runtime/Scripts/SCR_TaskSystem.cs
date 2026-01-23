using System;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    public static class TaskSystem
    {
        private const int MAX_TASKS = 1024;
        private static readonly SwapBackArray<TaskInstance> ACTIVE_TASKS = new(MAX_TASKS);
        private static GameObject ACTIVE_TASK_OBJECT = null;
        private static Updater ACTIVE_TASK_UPDATER = null;
        private static bool isShuttingDown = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            isShuttingDown = false;

            ACTIVE_TASK_OBJECT = null;
            ACTIVE_TASK_UPDATER = null;
            ACTIVE_TASKS.Clear();
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

            if (ACTIVE_TASK_OBJECT == null)
            {
                ACTIVE_TASK_OBJECT = new GameObject("TASK_UPDATER");
                ACTIVE_TASK_UPDATER = ACTIVE_TASK_OBJECT.AddComponent<Updater>();
                UnityEngine.Object.DontDestroyOnLoad(ACTIVE_TASK_OBJECT);
            }

            return true;
        }
        private static void Update()
        {
            int write = 0;
            for (int read = 0; read < ACTIVE_TASKS.Count; read++)
            {
                TaskInstance task = ACTIVE_TASKS[read];

                if (!task.IsCompleted)
                {
                    task.Update();
                    ACTIVE_TASKS[write++] = task;
                }
            }

            ACTIVE_TASKS.Truncate(write);
        }
        private static void Clear()
        {
            ACTIVE_TASKS.Clear();
        }
        public static bool TryCreate(TaskInstance taskInstance)
        {
            if (!IsValid())
            {
                return false;
            }

            ACTIVE_TASKS.Add(taskInstance);
            return true;
        }

        private class Updater : MonoBehaviour
        {
            [Header("_")]
            [SerializeField, ReadOnly] private int currentActiveTasks = -1;
            [SerializeField, ReadOnly] private int maximumActiveTasks = -1;

            private void Update()
            {
                currentActiveTasks = ACTIVE_TASKS.Count;
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