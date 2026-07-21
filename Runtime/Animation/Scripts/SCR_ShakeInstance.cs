using UnityEngine;

namespace Core.Animation
{
    using static CoreUtility;

    public sealed class ShakeInstance
    {
        private readonly SwapBackArray<ShakeState> collection;
        public ShakeInstance(uint capacity) => collection = new(capacity);

        public void Start(ShakeConfig config, float strength = 1f)
        {
            if (collection.Count == collection.Capacity)
            {
                ref ShakeState s = ref collection.GetRef(collection.Count - 1);
                s.Start(config, strength);
                return;
            }

            ShakeState state = new();
            state.Start(config, strength);
            collection.Add(state);
        }
        public Vector3 Update(float deltaTime)
        {
            Vector3 value = Vector3.zero;

            for (int i = collection.Count - 1; i >= 0; i--)
            {
                ref ShakeState s = ref collection.GetRef(i);

                if (!s.IsActive)
                {
                    collection.RemoveAt(i);
                }
                else
                {
                    value += s.Update(deltaTime);
                }
            }

            return value;
        }
        public void Clear() => collection.Clear();
    }
}