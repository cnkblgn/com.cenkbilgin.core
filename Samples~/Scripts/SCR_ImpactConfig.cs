using UnityEngine;
using Core;
using Core.Graphics;
using Core.Audio;

namespace Game
{
    [CreateAssetMenu(fileName = "SCO_Impact", menuName = "Resources/Impact Config", order = 0)]
    public class ImpactConfig : ScriptableObject
    {
        [Header("_")]
        [SerializeField] private ImpactData fallback = new();

        [Header("_")]
        [SerializeField] private ImpactData[] database = new ImpactData[] { new() };

        private int defaultLayerIndex = -1;
        private int terrainLayerIndex = -1;
        private int decalBitmask = -1;

#if UNITY_EDITOR
        public void Reset()
        {
            defaultLayerIndex = LayerMask.NameToLayer("Default");
            terrainLayerIndex = LayerMask.NameToLayer("Terrain");
            decalBitmask = (1 << defaultLayerIndex) | (1 << terrainLayerIndex);
        }
        private void OnValidate()
        {
            fallback.Validate();

            foreach (ImpactData data in database)
            {
                data.Validate();
            }
        }
#endif

        public void Spawn(Collider collider, Vector3 position, Vector3 normal, AudioGroup audioGroup, float audioVolume = 1, int audioBlend = 1, float audioMinDistance = 1, float audioMaxDistance = 100)
        {
            if (collider == null)
            {
                SpawnDecalInternal(fallback, collider, position, normal);
                SpawnParticleInternal(fallback, position, normal);
                SpawnAudioInternal(fallback, position, audioGroup, audioVolume, audioBlend, audioMinDistance, audioMaxDistance);
                return;
            }

            MaterialID id = collider.GetMaterial();

            foreach (ImpactData data in database)
            {
                if (data.ID.HasAny(id))
                {
                    SpawnDecalInternal(data, collider, position, normal);
                    SpawnParticleInternal(data, position, normal);
                    SpawnAudioInternal(data, position, audioGroup, audioVolume, audioBlend, audioMinDistance, audioMaxDistance);
                }
            }
        }
        private void SpawnAudioInternal(ImpactData impactData, Vector3 position, AudioGroup audioGroup, float audioVolume, int audioBlend, float audioMinDistance, float audioMaxDistance)
        {
            ManagerCoreAudio.Instance.PlaySound(impactData.ImpactSounds, audioGroup, position, audioBlend, audioVolume, 1, audioMinDistance, audioMaxDistance, true);
        }
        private void SpawnParticleInternal(ImpactData impactData, Vector3 position, Vector3 normal)
        {
            ManagerCoreGraphics.Instance.SpawnParticle(impactData.ImpactParticles, position, normal);
        }       
        private void SpawnDecalInternal(ImpactData impactData, Collider collider, Vector3 position, Vector3 normal)
        {
            if (collider != null && !collider.gameObject.IsInBitMask(decalBitmask))
            {
                return;
            }

            if (impactData.ImpactDecals != null && impactData.ImpactDecals.Length > 0)
            {
                ManagerCoreGraphics.Instance.SpawnDecal(impactData.ImpactDecals, collider != null ? collider.transform : null, position, Quaternion.Euler(Quaternion.LookRotation(-normal, Vector3.up).eulerAngles + (Vector3.forward * Random.Range(impactData.DecalMinRotation, impactData.DecalMaxRotation))), Random.Range(impactData.DecalMinScale, impactData.DecalMaxScale));
            }
        }
    }
}