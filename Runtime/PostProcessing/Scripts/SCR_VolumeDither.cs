using System;
using UnityEngine.Rendering;

namespace Core.PostProcessing
{
    [Serializable]
    [VolumeComponentMenu("Post-processing/Dither")]
    public sealed class Dither : VolumeComponent
    {
        public BoolParameter Enabled = new(false, true);
        public ClampedFloatParameter Strength = new(1, 0f, 2f, true);
        public ClampedFloatParameter Size = new(1, 0f, 1f, true);
    }
}
