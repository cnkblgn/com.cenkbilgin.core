using System.Collections.Generic;
using UnityEngine;

namespace Core.Misc
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(ParticleSystem))]
    public class ImpactEmitterParticle : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private ImpactConfig impactConfig = null;

        [Header("_")]
        [SerializeField] private LayerMask collisionMask = -1;
        [SerializeField] private bool isPersistent = false;

        private ParticleSystem thisParticleSystem = null;
        private GameObject particleLastGameobject = null;
        private Collider particleLastCollider = null;
        private List<ParticleCollisionEvent> particleCollisionEvents = null;

        public void Awake()
        {
            thisParticleSystem = GetComponent<ParticleSystem>();

            ParticleSystem.MainModule particleSystemMainModule = thisParticleSystem.main;
            ParticleSystem.EmissionModule particleSystemEmissionModule = thisParticleSystem.emission;
            ParticleSystem.CollisionModule particleSystemCollisionModule = thisParticleSystem.collision;

            particleSystemEmissionModule.enabled = true;

            particleSystemMainModule.playOnAwake = false;
            particleSystemMainModule.cullingMode = ParticleSystemCullingMode.Pause;
            particleSystemMainModule.simulationSpace = ParticleSystemSimulationSpace.World;
            particleSystemMainModule.ringBufferMode = isPersistent ? ParticleSystemRingBufferMode.PauseUntilReplaced : ParticleSystemRingBufferMode.Disabled;
                  
            particleSystemCollisionModule.enabled = true;

            particleCollisionEvents = new List<ParticleCollisionEvent>();
            particleSystemCollisionModule.maxCollisionShapes = particleSystemMainModule.maxParticles;
            particleSystemCollisionModule.sendCollisionMessages = true;
            particleSystemCollisionModule.collidesWith = collisionMask;
            particleSystemCollisionModule.dampenMultiplier = 1;
            particleSystemCollisionModule.bounceMultiplier = 0;
            particleSystemCollisionModule.lifetimeLossMultiplier = 1;
            particleSystemCollisionModule.type = ParticleSystemCollisionType.World;
        }
        private void OnParticleCollision(GameObject other)
        {
            if (impactConfig == null)
            {
                return;
            }

            ParticlePhysicsExtensions.GetCollisionEvents(thisParticleSystem, other, particleCollisionEvents);

            if (particleLastGameobject != other)
            {
                particleLastGameobject = other;
                particleLastCollider = particleLastGameobject.GetComponent<Collider>();
            }

            ParticleCollisionEvent collisionEvent = particleCollisionEvents[0];

            impactConfig.Spawn(particleLastCollider, collisionEvent.intersection, collisionEvent.normal, Audio.AudioGroup.EFFECT);
        }
    }
}