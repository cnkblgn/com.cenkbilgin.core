using UnityEngine;

namespace Core.Animation
{
    using static CoreUtility;

    public sealed class SpringInstance
    {
        private readonly SwapBackArray<SpringState> collection = default;
        public SpringInstance(uint capacity) => collection = new(capacity);

        public void Start(SpringConfig config, float strength = 1f)
        {
            if (collection.Count == collection.Capacity)
            {
                ref SpringState s = ref collection.GetRef(collection.Count - 1);
                s.Start(config, strength);
                return;
            }

            SpringState state = new();
            state.Start(config, strength);
            collection.Add(state);
        }
        public Vector3 Update(float deltaTime)
        {
            Vector3 value = Vector3.zero;

            for (int i = collection.Count - 1; i >= 0; i--)
            {
                ref SpringState state = ref collection.GetRef(i);

                if (!state.IsActive)
                {
                    collection.RemoveAt(i);
                }
                else
                {
                    value += state.Update(deltaTime);
                }
            }

            return value;
        }
        public void Clear() => collection.Clear();
    }
}