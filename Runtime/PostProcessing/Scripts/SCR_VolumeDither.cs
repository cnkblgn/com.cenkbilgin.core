using System;
using UnityEngine.Rendering;

namespace Core.PostProcessing
{
    [Serializable]
    [VolumeComponentMenu("Post-processing/Dither")]
    public sealed class VolumeDither : VolumeComponent
    {
        public BoolParameter Enabled = new(false, true);
        public ClampedFloatParameter Strength = new(1, 0f, 2f);
        public ClampedFloatParameter Size = new(1, 0f, 1f);
    }
}
