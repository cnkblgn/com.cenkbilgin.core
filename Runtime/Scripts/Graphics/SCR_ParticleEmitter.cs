using UnityEngine;

namespace Core.Graphics
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class ParticleEmitter : MonoBehaviour
    {
        private ParticleSystem[] thisEmitters = null;
        private Transform thisTransform = null;
        private int[] emitterCounts = null;
        private int lastEmitFrame = -1;

        private void Awake()
        {
            thisTransform = GetComponent<Transform>();
            thisEmitters = GetComponentsInChildren<ParticleSystem>();

            if (thisEmitters == null && thisEmitters.Length == 0)
            {
                LogError($"ParticleEmitter.Awake() {gameObject.name} needs [ParticleSystem] component in children!");
                return;
            }

            emitterCounts = new int[thisEmitters.Length];

            for (int i = 0; i < emitterCounts.Length; i++)
            {
                ParticleSystem.EmissionModule emissionModule = thisEmitters[i].emission;

                if (emissionModule.burstCount <= 0)
                {
                    emitterCounts[i] = (int)emissionModule.rateOverTime.constant;
                }
                else
                {
                    emitterCounts[i] = (int)emissionModule.GetBurst(0).count.constant;
                }                
            }
        }
        public void Emit(Vector3 position, Quaternion rotation, Vector3 normal)
        {
            thisTransform.SetPositionAndRotation(position, rotation);
            thisTransform.LookAt(position + normal);

            Emit();
        }
        public void Emit()
        {
            if (lastEmitFrame == Time.frameCount)
            {
                return;
            }

            for (int i = 0; i < thisEmitters.Length; i++)
            {
                thisEmitters[i].Emit(emitterCounts[i]);
            }

            lastEmitFrame = Time.frameCount;
        }
        public void Play()
        {
            for (int i = 0; i < thisEmitters.Length; i++)
            {
                thisEmitters[i].Play();
            }
        }
        public void Clear()
        {
            for (int i = 0; i < thisEmitters.Length; i++)
            {
                thisEmitters[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        public void Stop()
        {
            for (int i = 0; i < thisEmitters.Length; i++)
            {
                thisEmitters[i].Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}