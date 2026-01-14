using System;

namespace Core
{
    public class DebugCommandInstanceBase
    {
        public readonly string ID;
        public readonly string Description;
        public readonly string Format;

        public DebugCommandInstanceBase(string id, string description, string format)
        => (ID, Description, Format) = (id, description, format);
    }
    public class DebugCommandInstance : DebugCommandInstanceBase
    {
        private event Action Action = null;
        public DebugCommandInstance(string id, string description, string format, Action command) : base(id, description, format) => this.Action = command;
        public void Execute() => Action?.Invoke();
    }
    public class DebugCommandInstance<T1> : DebugCommandInstanceBase
    {
        private event Action<T1> Action = null;
        public DebugCommandInstance(string id, string description, string format, Action<T1> command) : base(id, description, format) => this.Action = command;
        public void Execute(T1 value) => Action?.Invoke(value);
    }
    public class DebugCommandInstance<T1, T2> : DebugCommandInstanceBase
    {
        private event Action<T1, T2> Action = null;
        public DebugCommandInstance(string id, string description, string format, Action<T1, T2> command) : base(id, description, format) => this.Action = command;
        public void Execute(T1 value_01, T2 value_02) => Action?.Invoke(value_01, value_02);
    }
}