using System;
using UnityEngine.Rendering;

namespace Core.PostProcessing
{
    [Serializable]
    [VolumeComponentMenu("Post-processing/Quantize")]
    public sealed class VolumeQuantize : VolumeComponent
    {
        public BoolParameter Enabled = new(false, true);
        public ClampedIntParameter Steps = new(16, 2, 128);
    }
}
