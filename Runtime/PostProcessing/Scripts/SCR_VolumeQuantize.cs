using System;
using UnityEngine.Rendering;

namespace Core.PostProcessing
{
    [Serializable]
    [VolumeComponentMenu("Image/Quantize")]
    public sealed class Quantize : VolumeComponent
    {
        public BoolParameter Enabled = new(false, true);
        public ClampedIntParameter Steps = new(16, 2, 128, true);
    }
}
