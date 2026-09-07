using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.PostProcessing
{
    [Serializable]
    [VolumeComponentMenu("Post-processing/Vibrance")]
    public sealed class VolumeVibrance : VolumeComponent
    {
        public BoolParameter Enabled = new(false, true);
        public ClampedFloatParameter Intensity = new(1, 0f, 10f);
        public Vector3Parameter Balance = new(Vector3.one, true);
    }
}
