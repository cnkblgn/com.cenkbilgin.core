using System;
using UnityEngine.Rendering;

namespace Core.PostProcessing
{
    [Serializable]
    [VolumeComponentMenu("Post-processing/Dither")]
    public sealed class Dither : VolumeComponent
    {
        public BoolParameter Enabled = new(false, true);
        public ClampedFloatParameter Strength = new(1, 0f, 1f, true);
        public ClampedFloatParameter Size = new(1, 0f, 8f, true);
        public ClampedFloatParameter Spread = new(0.1f, 0f, 1f, true);
        public ClampedFloatParameter Steps = new(12, 1, 32, true);
    }
}
