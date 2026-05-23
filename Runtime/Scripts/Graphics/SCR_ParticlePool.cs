using UnityEngine;

namespace Core.Graphics
{
    public sealed class ParticlePool : IPoolHandler<ParticleEmitter>
    {
        public readonly PoolSystem<ParticleEmitter> Pool;

        public ParticlePool(PoolType type, ParticleEmitter prefab, Transform container, int count) => Pool = new("PARTICLE_POOL", type, prefab, container, count, this);

        public ParticleEmitter Spawn(Vector3 position, Vector3 direction)
        {
            ParticleEmitter emitter = Pool.GetNext();
            emitter.gameObject.SetActive(true);
            emitter.Emit(position, direction);
            return emitter;
        }

        public void OnInitialize(ParticleEmitter emitter) 
        {
            ParticleSystem[] emitters = emitter.GetComponentsInChildren<ParticleSystem>();

            for (int j = 0; j < emitters.Length; j++)
            {
                ParticleSystem.MainModule mainModule = emitters[j].main;
                mainModule.stopAction = ParticleSystemStopAction.None;
                mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
            }
        }
        public void OnReset(ParticleEmitter emitter) 
        {
            emitter.Clear();
        }
    }
}
