using System;
using UnityEngine;

namespace Core.Graphics
{
    public readonly struct MeshBinding
    {
        public readonly MeshRenderer Renderer;
        public readonly MeshFilter Filter;

        public MeshBinding(MeshRenderer renderer, MeshFilter filter)
        {
            Renderer = renderer != null ? renderer : throw new ArgumentNullException(nameof(renderer), "Mesh binding ctor failed! renderer missing!?");
            Filter = filter != null ? filter : throw new ArgumentNullException(nameof(filter), "Mesh binding ctor failed! filter missing!?");
        }
    }
}
