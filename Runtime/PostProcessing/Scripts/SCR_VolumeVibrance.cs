using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.PostProcessing
{
    [Serializable]
    [VolumeComponentMenu("Post-processing/Vibrance")]
    public sealed class Vibrance : VolumeComponent
    {
        public BoolParameter Enabled = new(false, true);
        public ClampedFloatParameter Strength = new(1, 0f, 10f, true);
        public Vector3Parameter Balance = new(Vector3.one, true);
    }
}
