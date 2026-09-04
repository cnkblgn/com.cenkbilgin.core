using System;

namespace Core.UI
{
    public readonly struct UIContextItemContext
    {
        internal readonly string Name;
        internal readonly Action Action;

        public UIContextItemContext(string name, Action action)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "UI context item ctx failed! name cannot be null!");
            Action = action ?? throw new ArgumentNullException(nameof(action), "UI context item ctx failed! action cannot be null!");
        }
    }
}