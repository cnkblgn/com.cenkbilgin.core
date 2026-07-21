using UnityEngine;

namespace Core.Surface
{
    [DisallowMultipleComponent]
    public abstract class Surface : MonoBehaviour
    {
        public abstract bool TryGetSurface(in SurfaceContext ctx, out ulong tag);
    }
}
