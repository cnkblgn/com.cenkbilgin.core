using UnityEngine;

namespace Core.Surface
{
    using static CoreUtility;

    public class SurfaceMap : ScriptableObject
    {
        [SerializeField, ReadOnly] private Vector3 origin;
        [SerializeField, ReadOnly] private Vector3 size;
        [SerializeField, ReadOnly] private int width;
        [SerializeField, ReadOnly] private int height;

        [SerializeField, HideInInspector] private ulong[] tags;

        public void Initialize(Vector3 origin, Vector3 size, int width, int height, ulong[] tags)
        {
            this.origin = origin;
            this.size = size;
            this.width = width;
            this.height = height;
            this.tags = tags;
        }

        public ulong GetTag(Vector3 worldPosition)
        {
            float u = Mathf.Clamp01((worldPosition.x - origin.x) / size.x);
            float v = Mathf.Clamp01((worldPosition.z - origin.z) / size.z);

            int x = Mathf.Min((int)(u * width), width - 1);
            int y = Mathf.Min((int)(v * height), height - 1);

            return tags[x + y * width];
        }
    }
}
