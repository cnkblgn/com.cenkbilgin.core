using UnityEngine;

namespace Core.Surface
{
    public sealed class SurfaceStatic : Surface
    {
        public ulong Tags { get; private set; }

        [Header("_")]
        [SerializeField] private SurfaceTag[] tags;

        private void Awake() => Tags = SurfaceTag.CreateMask(tags);

        public override bool TryGetSurface(in SurfaceContext ctx, out ulong tag)
        {
            tag = Tags;

            return true;
        }
    }
}
