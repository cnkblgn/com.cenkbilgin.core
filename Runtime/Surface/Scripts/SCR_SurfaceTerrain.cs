using System;
using UnityEngine;

namespace Core.Surface
{
    public sealed class SurfaceTerrain : Surface
    {
        [Header("_")]
        [SerializeField, Required] private SurfaceMap map;

        private void Awake()
        {
            if (map == null)
            {
                throw new NullReferenceException($"Terrain surface map is missing! {nameof(map)}");
            }
        }

        public override bool TryGetSurface(in SurfaceContext ctx, out SurfaceMask mask)
        {
            mask = new(map.GetTag(ctx.Position));
            return true;
        }
    }
}
