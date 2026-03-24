using System.Collections.Generic;
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
        [SerializeField] private ImpactData fallback = null;

        [Header("_")]
        [SerializeField] private List<ImpactData> database = new() { new() };

        private readonly Dictionary<string, ImpactData> table = new();
        private int defaultLayerIndex = -1;
        private int terrainLayerIndex = -1;
        private int decalBitmask = -1;

#if UNITY_EDITOR
        private bool IsExists(string tag)
        {
            foreach (ImpactData config in database)
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
            PopulateLayers();

            if (database == null)
            {
                Reset();
                return;
            }

            PopulateDatabase();
            PopulateTable();
        }
        private void PopulateLayers()
        {
            defaultLayerIndex = LayerMask.NameToLayer("Default");
            terrainLayerIndex = LayerMask.NameToLayer("Terrain");
            decalBitmask = (1 << defaultLayerIndex) | (1 << terrainLayerIndex);
        }
        private void PopulateDatabase()
        {
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

            database.AddRange(effects);

        }
        private void PopulateTable()
        {
            table.Clear();

            foreach (ImpactData config in database)
            {
                table[config.Name] = config;
            }

        }
        public void Reset()
        {
            database ??= new();
            database.Clear();

            PopulateLayers();
            PopulateDatabase();
            PopulateTable();
        }
        private void OnValidate() => PopulateTable();
#endif

        public void Spawn(Collider collider, Vector3 position, Vector3 normal, AudioGroup audioGroup, float audioVolume = 1, int audioBlend = 1)
        {
            if (collider != null && table.TryGetValue(collider.tag, out ImpactData data))
            {
                SpawnDecalInternal(data, collider, position, normal);
                SpawnParticleInternal(data, position, normal);
                SpawnAudioInternal(data, position, audioGroup, audioVolume, audioBlend);
            }
            else
            {
                SpawnDecalInternal(fallback, collider, position, normal);
                SpawnParticleInternal(fallback, position, normal);
                SpawnAudioInternal(fallback, position, audioGroup, audioVolume, audioBlend);
            }
        }
        private void SpawnAudioInternal(ImpactData impactData, Vector3 position, AudioGroup audioGroup, float audioVolume, int audioBlend)
        {
            ManagerCoreAudio.Instance.PlaySound(impactData.ImpactSounds, audioGroup, position, audioBlend, audioVolume, 1, 1, 100, true);
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