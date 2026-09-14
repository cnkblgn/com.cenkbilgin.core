using System.Collections.Generic;
using UnityEngine;

namespace Core.UI
{
    public static class UICursorSystem
    {
        internal static readonly List<IUICursorStateHandler> Handlers = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize() => Handlers.Clear();

        public static void BindHandler(IUICursorStateHandler value)
        {
            if (Handlers.Contains(value))
            {
                return;
            }

            Handlers.Add(value);
        }
        public static void UnbindHandler(IUICursorStateHandler value)
        {
            if (!Handlers.Contains(value))
            {
                return;
            }

            Handlers.Remove(value);
        }
    }
}