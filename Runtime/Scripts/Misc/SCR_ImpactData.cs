using System;
using UnityEngine;
using Core.Graphics;
using Core.Audio;

namespace Core.Misc
{
    using static CoreUtility;

    [Serializable]
    public class ImpactData
    {
        [HideInInspector] public string Name;
        public ParticleEmitter ImpactParticle => impactParticle;
        public AudioClip[] ImpactSounds => impactSounds;
        public DecalEmitter[] ImpactDecals => impactDecals;
        public float DecalMinScale => decalMinScale;
        public float DecalMaxScale => decalMaxScale;
        public float DecalMinRotation => decalMinRotation;
        public float DecalMaxRotation => decalMaxRotation;

        [SerializeField] private ParticleEmitter impactParticle = null;
        [SerializeField] private AudioClip[] impactSounds = null;
        [SerializeField] private DecalEmitter[] impactDecals = null;
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

            this.impactParticle = data.impactParticle;
            this.impactDecals = data.impactDecals;
            this.impactSounds = data.impactSounds;
            this.decalMinScale = data.decalMinScale;
            this.decalMaxScale = data.decalMaxScale;
            this.decalMinRotation = data.decalMinRotation;
            this.decalMaxRotation = data.decalMaxRotation;
        }
    }
}
