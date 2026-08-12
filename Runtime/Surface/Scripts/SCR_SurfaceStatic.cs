using System.Collections.Generic;
using UnityEngine;

namespace Core.Surface
{
    public sealed class SurfaceStatic : Surface
    {
        public ulong Tags { get; private set; }

        [Header("_")]
        [SerializeField] private SurfaceTag[] tags;

        private void Awake() => SetTags(tags);

        public IReadOnlyList<SurfaceTag> GetTags() => tags;
        public void SetTags(SurfaceTag[] tags)
        {
            this.tags = new SurfaceTag[tags.Length];

            for (int i = 0; i < tags.Length; i++)
            {
                this.tags[i] = tags[i];
            }

            Tags = SurfaceTag.CreateMask(tags);
        }

        public override bool TryGetSurface(in SurfaceContext ctx, out ulong tag)
        {
            tag = Tags;

            return true;
        }
    }
}
