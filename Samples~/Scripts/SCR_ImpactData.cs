using System;
using UnityEngine;
using Core;
using Core.Graphics;

namespace Game
{
    using static CoreUtility;

    [Serializable]
    public class ImpactData
    {
        [HideInInspector] public string Name;
        public ParticleEmitter[] ImpactParticles => particles;
        public AudioClip[] ImpactSounds => sounds;
        public DecalEmitter[] ImpactDecals => decals;
        public float DecalMinScale => decalMinScale;
        public float DecalMaxScale => decalMaxScale;
        public float DecalMinRotation => decalMinRotation;
        public float DecalMaxRotation => decalMaxRotation;

        [SerializeField] private ParticleEmitter[] particles = null;
        [SerializeField] private AudioClip[] sounds = null;
        [SerializeField] private DecalEmitter[] decals = null;
        [SerializeField, Min(0.01f)] private float decalMinScale = 1f;
        [SerializeField, Min(0.01f)] private float decalMaxScale = 2f;
        [SerializeField, Range(0, 359.9f)] private float decalMinRotation = 0f;
        [SerializeField, Range(0, 359.9f)] private float decalMaxRotation = 359f;

        public ImpactData() { }
        public ImpactData(ImpactData data)
        {
            if (data == null)
            {
                return;
            }

            this.particles = data.particles;
            this.decals = data.decals;
            this.sounds = data.sounds;
            this.decalMinScale = data.decalMinScale;
            this.decalMaxScale = data.decalMaxScale;
            this.decalMinRotation = data.decalMinRotation;
            this.decalMaxRotation = data.decalMaxRotation;
        }
    }
}
