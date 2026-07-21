using System;
using UnityEngine;

namespace Core.Surface
{
    using static CoreUtility;

    public sealed class SurfaceTerrain : Surface
    {
        [Header("_")]
        [SerializeField, Required] private SurfaceMap map;

        private void Awake()
        {
            if (map == null)
            {
                throw new NullReferenceException($"terrain surface map is missing! {nameof(map)}");
            }
        }

        public override bool TryGetSurface(in SurfaceContext ctx, out ulong tag)
        {
            tag = map.GetTag(ctx.Position);

            return true;
        }
    }
}
