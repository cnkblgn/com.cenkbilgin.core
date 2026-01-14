using System;
using UnityEngine;

namespace Core.Graphics
{
    using static CoreUtility;

    [Serializable]
    public class PoolSystemParticle : PoolSystem<ParticleEmitter>
    {
        protected override void OnInitialize(ParticleEmitter item)
        {
            ParticleSystem[] emitters = item.GetComponentsInChildren<ParticleSystem>();

            for (int j = 0; j < emitters.Length; j++)
            {
                ParticleSystem.MainModule mainModule = emitters[j].main;
                mainModule.stopAction = ParticleSystemStopAction.None;
                mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
            }
        }
        protected override void OnReset(ParticleEmitter item) => item.Clear();
        public ParticleEmitter Spawn(Vector3 position, Quaternion rotation, Vector3 normal)
        {
            ParticleEmitter particleEmitter = GetNext();
            particleEmitter.gameObject.SetActive(true);
            particleEmitter.Emit(position, rotation, normal);
            return particleEmitter;
        }
    }
}