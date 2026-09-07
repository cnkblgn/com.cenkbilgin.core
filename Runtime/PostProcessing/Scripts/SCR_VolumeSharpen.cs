using System;
using UnityEngine.Rendering;

namespace Core.PostProcessing
{
    [Serializable]
    [VolumeComponentMenu("Post-processing/Sharpen")]
    public sealed class VolumeSharpen : VolumeComponent
    {
        public BoolParameter Enabled = new(false, true);
        public ClampedFloatParameter Strength = new(1f, 0f, 10f);
        public ClampedFloatParameter Offset = new(0.5f, 0.1f, 4f);
        public ClampedFloatParameter Clamp = new(0.03f, 0f, 2f);
    }
}
