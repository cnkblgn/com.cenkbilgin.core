using System;
using UnityEngine.Rendering;

namespace Core.PostProcessing
{
    [Serializable]
    [VolumeComponentMenu("Image/Clarify")]
    public sealed class Clarify : VolumeComponent
    {
        public BoolParameter Enabled = new(false, true);
        public ClampedFloatParameter Strength = new(2, 0f, 10f, true);
    }
}
