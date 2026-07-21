using UnityEngine;

namespace Core.Graphics
{
    [DisallowMultipleComponent]
    public sealed class ParticleEmitter : MonoBehaviour
    {
        private Transform thisTransform = null;
        private ParticleSystem[] thisEmitters = null;
        private int[] emitterCounts = null;
        private int lastEmitFrame = -1;

        private void Awake()
        {
            thisTransform = GetComponent<Transform>();
            thisEmitters = GetComponentsInChildren<ParticleSystem>();

            if (thisEmitters == null || thisEmitters.Length == 0)
            {
                Debug.LogError($"{gameObject.name} needs [ParticleSystem] component in children!");
                return;
            }

            emitterCounts = new int[thisEmitters.Length];

            for (int i = 0; i < thisEmitters.Length; i++)
            {
                ParticleSystem.EmissionModule m = thisEmitters[i].emission;

                emitterCounts[i] = m.burstCount <= 0 ? 0 : (int)m.GetBurst(0).count.constant;
            }
        }

        private void Align(Vector3 position, Vector3 direction) => thisTransform.SetPositionAndRotation(position, Quaternion.LookRotation(direction, Vector3.up));

        public void Emit(Vector3 position, Vector3 direction)
        {
            if (lastEmitFrame == Time.frameCount)
            {
                return;
            }

            Align(position, direction);

            for (int i = 0; i < thisEmitters.Length; i++)
            {
                thisEmitters[i].Emit(emitterCounts[i]);
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
        public void Play(Vector3 position, Vector3 direction)
        {
            Align(position, direction);

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