using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Core.Surface.Editor
{
    using static CoreUtility;

    public enum Resolution : int
    {
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
    }

    [Serializable]
    public struct Mapping
    {
        public TerrainLayer Layer;
        public SurfaceTag[] Tags;
    }

    [RequireComponent(typeof(Terrain))]
    internal class SurfaceTerrainBaker : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private Resolution resolution = Resolution._512;
        [SerializeField] private Mapping[] mappings;

        [Clickable("Build")]
        internal void Bake()
        {
            Terrain terrain = GetComponent<Terrain>();
            TerrainData data = terrain.terrainData;

            if (data.terrainLayers.Length <= 0)
            {
                Debug.LogWarning("Terrain surface bake failed! Terrain does not have any terrain layer registered! please assign layers to terrain!");
                return;
            }

            int resolution = (int)this.resolution;

            float[,,] alpha = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);

            Dictionary<TerrainLayer, ulong> lookup = new();

            foreach (var mapping in mappings)
            {
                lookup[mapping.Layer] = SurfaceTag.CreateMask(mapping.Tags);
            }

            ulong[] tags = new ulong[resolution * resolution];

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int ax = Mathf.RoundToInt((float)x / (resolution - 1) * (data.alphamapWidth - 1));
                    int ay = Mathf.RoundToInt((float)y / (resolution - 1) * (data.alphamapHeight - 1));

                    int bestLayer = 0;
                    float bestWeight = 0f;

                    for (int layer = 0; layer < data.terrainLayers.Length; layer++)
                    {
                        float weight = alpha[ay, ax, layer];

                        if (weight > bestWeight)
                        {
                            bestWeight = weight;
                            bestLayer = layer;
                        }
                    }

                    TerrainLayer terrainLayer = data.terrainLayers[bestLayer];

                    if (!lookup.TryGetValue(terrainLayer, out ulong tag))
                    {
                        tag = 0;
                    }

                    tags[x + y * resolution] = tag;
                }
            }

            if (TryCreateAsset<SurfaceMap>("Assets/", out SurfaceMap map))
            {
                map.Initialize(terrain.transform.position, data.size, resolution, resolution, tags);
                EditorUtility.SetDirty(map);
                AssetDatabase.SaveAssets();
                Debug.Log("Surface map baked.");
            }
        }
    }
}
