using System;

namespace Core
{
    public static class DebugCommandLogger
    {
        public static event Action<string> OnLogReceived = null;
        public static event Action OnLogCleared = null;
        public static void Log(string value) => OnLogReceived?.Invoke(value);
        public static void Clear() => OnLogCleared?.Invoke();
    }
}