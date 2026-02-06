using System.Collections.Generic;
using UnityEngine;
using Core.Graphics;
using Core.Audio;

namespace Core.Misc
{
    [CreateAssetMenu(fileName = "SCO_Impact", menuName = "Resources/Impact Config", order = 0)]
    public class ImpactConfig : ScriptableObject
    {
        [Header("_")]
        [SerializeField] private ImpactData fallbackConfig = null;

        [Header("_")]
        [SerializeField] private List<ImpactData> configs = new() { new() };

        private int defaultLayerIndex = -1;
        private int terrainLayerIndex = -1;
        private int decalBitmask = -1;

#if UNITY_EDITOR
        private bool IsExists(string tag)
        {
            foreach (ImpactData config in configs)
            {
                if (string.Equals(config.Name, tag))
                {
                    return true;
                }
            }

            return false;
        }

        [ContextMenu("Update Tags")]
        public void Update()
        {
            defaultLayerIndex = LayerMask.NameToLayer("Default");
            terrainLayerIndex = LayerMask.NameToLayer("Terrain");
            decalBitmask = (1 << defaultLayerIndex) | (1 << terrainLayerIndex);

            if (configs == null)
            {
                Reset();
                return;
            }

            string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
            List<ImpactData> effects = new();

            foreach (string tag in tags)
            {
                if (!tag.Contains("Material/"))
                {
                    continue;
                }

                if (IsExists(tag))
                {
                    continue;
                }

                effects.Add(new(null) { Name = tag });
            }

            configs.AddRange(effects);
        }
        public void Reset()
        {
            defaultLayerIndex = LayerMask.NameToLayer("Default");
            terrainLayerIndex = LayerMask.NameToLayer("Terrain");
            decalBitmask = (1 << defaultLayerIndex) | (1 << terrainLayerIndex);

            configs ??= new();
            configs.Clear();

            string[] tags = UnityEditorInternal.InternalEditorUtility.tags;

            foreach (string tag in tags)
            {
                if (!tag.Contains("Material/"))
                {
                    continue;
                }

                ImpactData effect = new(null)
                {
                    Name = tag,
                };

                configs.Add(effect);
            }
        }
#endif

        public void Spawn(Collider collider, Vector3 position, Vector3 normal, AudioGroup audioGroup, float audioVolume = 1, int audioBlend = 1)
        {
            if (collider == null)
            {
                SpawnDecalInternal(fallbackConfig, collider, position, normal);
                SpawnParticleInternal(fallbackConfig, position, normal);
                SpawnAudioInternal(fallbackConfig, position, audioGroup, audioVolume, audioBlend);
                return;
            }

            for (int i = 0; i < configs.Count; i++)
            {
                if (!collider.CompareTag(configs[i].Name))
                {
                    continue;
                }

                SpawnDecalInternal(configs[i], collider, position, normal);
                SpawnParticleInternal(configs[i], position, normal);
                SpawnAudioInternal(configs[i], position, audioGroup, audioVolume, audioBlend);
                return;
            }

            SpawnDecalInternal(fallbackConfig, collider, position, normal);
            SpawnParticleInternal(fallbackConfig, position, normal);
            SpawnAudioInternal(fallbackConfig, position, audioGroup, audioVolume, audioBlend);
        }
        private void SpawnAudioInternal(ImpactData impactData, Vector3 position, AudioGroup audioGroup, float audioVolume, int audioBlend)
        {
            if (impactData.ImpactSounds != null && impactData.ImpactSounds.Length > 0)
            {
                ManagerCoreAudio.Instance.PlaySound(impactData.ImpactSounds, audioGroup, position, audioBlend, audioVolume, 1, 1, 100, true);
            }
        }
        private void SpawnParticleInternal(ImpactData impactData, Vector3 position, Vector3 normal)
        {
            if (impactData.ImpactParticle == null)
            {
                return;
            }

            ManagerCoreGraphics.Instance.SpawnParticle(impactData.ImpactParticle, position, Quaternion.identity, normal);
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