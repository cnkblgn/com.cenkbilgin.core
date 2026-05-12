using UnityEngine;

namespace Core.Graphics
{
    public sealed class PoolSystemParticle : PoolSystem<ParticleEmitter>
    {
        public override PoolType Type => PoolType.SINGLE;
        public override string ID => "PARTICLE_POOL";

        protected sealed override void OnInitialize(ParticleEmitter item)
        {
            ParticleSystem[] emitters = item.GetComponentsInChildren<ParticleSystem>();

            for (int j = 0; j < emitters.Length; j++)
            {
                ParticleSystem.MainModule mainModule = emitters[j].main;
                mainModule.stopAction = ParticleSystemStopAction.None;
                mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
            }
        }
        protected sealed override void OnReset(ParticleEmitter item) => item.Clear();

        public ParticleEmitter Spawn(Vector3 position, Vector3 direction)
        {
            ParticleEmitter particleEmitter = GetNext();
            particleEmitter.gameObject.SetActive(true);
            particleEmitter.Emit(position, direction);
            return particleEmitter;
        }
    }
}