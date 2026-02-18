using UnityEngine;

namespace Core.Graphics
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class ParticleEmitter : MonoBehaviour
    {
        private ParticleSystem[] thisEmitters = null;
        private int[] emitterCounts = null;
        private int lastEmitFrame = -1;

        private void Awake()
        {
            thisEmitters = GetComponentsInChildren<ParticleSystem>();

            if (thisEmitters == null || thisEmitters.Length == 0)
            {
                Debug.LogError($"ParticleEmitter.Awake() {gameObject.name} needs [ParticleSystem] component in children!");
                return;
            }

            emitterCounts = new int[thisEmitters.Length];

            for (int i = 0; i < emitterCounts.Length; i++)
            {
                emitterCounts[i] = (int)thisEmitters[i].emission.GetBurst(0).count.Evaluate(0, Random.value);
            }
        }

        public void Emit(Vector3 position, Vector3 direction)
        {
            if (lastEmitFrame == Time.frameCount)
            {
                return;
            }

            for (int i = 0; i < thisEmitters.Length; i++)
            {
                thisEmitters[i].Emit
                (
                    new()
                    {
                        position = position,
                        rotation3D = Quaternion.LookRotation(direction).eulerAngles
                    },
                    emitterCounts[i]
                );
            }

            lastEmitFrame = Time.frameCount;
        }
        public void Emit(in ParticleSystem.EmitParams emitParams)
        {
            if (lastEmitFrame == Time.frameCount)
            {
                return;
            }

            for (int i = 0; i < thisEmitters.Length; i++)
            {
                thisEmitters[i].Emit(emitParams, emitterCounts[i]);
            }

            lastEmitFrame = Time.frameCount;
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