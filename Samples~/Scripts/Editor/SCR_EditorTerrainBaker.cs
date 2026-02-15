using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Core.Misc.Editor
{
    using static CoreUtility;

    [RequireComponent(typeof(Terrain))]
    public class EditorTerrainBaker : MonoBehaviour
    {
        private enum Size
        {
            _2 = 2,
            _4 = 4,
            _8 = 8,
            _16 = 16,
            _32 = 32,
        }
        private enum Resolution
        {
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512,
        }

        [Header("_")]
        [SerializeField] private Size chunkSize = Size._4;
        [SerializeField] private Resolution chunkResolution = Resolution._512;

        [Header("_")]
        [SerializeField, Required] private string exportFolder = "Assets/_TerrainBaker";

        [Header("_")]
        [SerializeField, Required] private Material terrainMaterial = null;

        [ContextMenu("Bake")]
        public void BakeAndSave() => Bake(GetComponent<Terrain>(), terrainMaterial, (int)chunkResolution, (int)chunkSize, exportFolder);

        private void ExportSplats(TerrainData terrainData, string exportFolder)
        {
            int width = terrainData.alphamapWidth;
            int height = terrainData.alphamapHeight;
            int layers = terrainData.alphamapLayers;

            TerrainLayer[] terrainLayers = terrainData.terrainLayers;
            float[,,] splatMap = terrainData.GetAlphamaps(0, 0, width, height);

            for (int i = 0; i < layers; i++)
            {
                string layerName = (i < terrainLayers.Length && terrainLayers[i] != null) ? terrainLayers[i].name.Replace(" ", "_") : $"layer{i}";

                Texture2D layerTexture = new(width, height, TextureFormat.RFloat, false);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        layerTexture.SetPixel(x, y, new Color(splatMap[y, x, i], splatMap[y, x, i], splatMap[y, x, i], 1f));
                    }
                }

                layerTexture.Apply();

                string fileName = $"splat_{i}_{layerName}.png";
                string filePath = Path.Combine(exportFolder, fileName);

                File.WriteAllBytes(filePath, layerTexture.EncodeToPNG());
                AssetDatabase.ImportAsset(filePath);
            }
        }
        private void ExportMesh(Mesh mesh, string meshName, string exportFolder)
        {
            string assetPath = Path.Combine(exportFolder, $"{meshName}.asset");

            AssetDatabase.CreateAsset(mesh, assetPath);
        }

        private void Bake(Terrain terrainObject, Material terrainMaterial, int chunkResolution, int chunkSize, string exportFolder)
        {
            if (!Directory.Exists(exportFolder))
            {
                Directory.CreateDirectory(exportFolder);
                AssetDatabase.Refresh();
            }

            List<GameObject> chunkObjects = BakeInternal(terrainObject, terrainMaterial, chunkResolution, chunkSize);

            foreach (GameObject chunkObject in chunkObjects)
            {
                ExportMesh(chunkObject.GetComponent<MeshFilter>().sharedMesh, chunkObject.name, exportFolder);
            }

            ExportSplats(terrainObject.terrainData, exportFolder);

            AssetDatabase.Refresh();
        }
        private List<GameObject> BakeInternal(Terrain terrainObject, Material terrainMaterial, int chunkResolution, int chunkSize)
        {
            chunkResolution = Mathf.Clamp(chunkResolution, 2, 1024);
            chunkSize = Mathf.Clamp(chunkSize, 1, 32);

            TerrainData terrainData = terrainObject.terrainData;
            Vector3 terrainSize = terrainData.size;
            Vector3 terrainPosition = terrainObject.GetPosition();
            terrainPosition.y *= 2;

            List<GameObject> chunkObjects = new();
            Transform chunkRoot = new GameObject("Root").transform;

            int verticesPerChunk = Mathf.CeilToInt(chunkResolution / (float)chunkSize);
            int terrainResolution = verticesPerChunk * chunkSize + 1;

            float[,] heightmap = CalculateHeightMap(terrainObject, terrainSize, terrainResolution);
            Vector3[,] normalMap = CalculateNormalMap(heightmap, terrainSize, terrainResolution);

            // Create chunks
            for (int chunkY = 0; chunkY < chunkSize; chunkY++)
            {
                for (int chunkX = 0; chunkX < chunkSize; chunkX++)
                {
                    GameObject chunkObject = new($"Chunk_{chunkX}_{chunkY}");
                    MeshFilter meshFilter = chunkObject.AddComponent<MeshFilter>();
                    MeshRenderer meshRenderer = chunkObject.AddComponent<MeshRenderer>();

                    Mesh mesh = new() { name = $"Mesh_{chunkX}_{chunkY}" };

                    int startX = chunkX * verticesPerChunk;
                    int startY = chunkY * verticesPerChunk;
                    int endX = Mathf.Min(startX + verticesPerChunk + 1, terrainResolution);
                    int endY = Mathf.Min(startY + verticesPerChunk + 1, terrainResolution);
                    int chunkWidth = endX - startX;
                    int chunkHeight = endY - startY;

                    Vector3[] vertices = new Vector3[chunkWidth * chunkHeight];
                    Vector3[] normals = new Vector3[vertices.Length];
                    Vector2[] uv = new Vector2[vertices.Length];
                    int[] triangles = new int[(chunkWidth - 1) * (chunkHeight - 1) * 6];

                    for (int y = 0, i = 0; y < chunkHeight; y++)
                    {
                        float yPosition = (float)(startY + y) / (terrainResolution - 1);

                        for (int x = 0; x < chunkWidth; x++, i++)
                        {
                            float xPosition = (float)(startX + x) / (terrainResolution - 1);

                            vertices[i] = new Vector3(xPosition * terrainSize.x, heightmap[startX + x, startY + y], yPosition * terrainSize.z);

                            uv[i] = new Vector2(xPosition, yPosition);
                            normals[i] = normalMap[startX + x, startY + y];
                        }
                    }

                    for (int y = 0, triIndex = 0, vertIndex = 0; y < chunkHeight - 1; y++, vertIndex++)
                    {
                        for (int x = 0; x < chunkWidth - 1; x++, triIndex += 6, vertIndex++)
                        {
                            triangles[triIndex] = vertIndex;
                            triangles[triIndex + 1] = vertIndex + chunkWidth;
                            triangles[triIndex + 2] = vertIndex + 1;
                            triangles[triIndex + 3] = vertIndex + 1;
                            triangles[triIndex + 4] = vertIndex + chunkWidth;
                            triangles[triIndex + 5] = vertIndex + chunkWidth + 1;
                        }
                    }

                    mesh.vertices = vertices;
                    mesh.uv = uv;
                    mesh.normals = normals;
                    mesh.triangles = triangles;
                    mesh.RecalculateBounds();

                    meshFilter.mesh = mesh;
                    meshRenderer.sharedMaterial = terrainMaterial;

                    chunkObject.transform.position = terrainPosition;
                    chunkObject.transform.parent = chunkRoot;
                    chunkObjects.Add(chunkObject);
                }
            }

            return chunkObjects;
        }

        private float[,] CalculateHeightMap(Terrain terrainObject, Vector3 terrainSize, int terrainResolution)
        {
            float[,] heightMap = new float[terrainResolution, terrainResolution];
            Vector3 terrainPosition = terrainObject.GetPosition(); // Get actual terrain position

            for (int y = 0; y < terrainResolution; y++)
            {
                float yPosition = (float)y / (terrainResolution - 1);

                for (int x = 0; x < terrainResolution; x++)
                {
                    float xPosition = (float)x / (terrainResolution - 1);

                    // Correct world position calculation:
                    Vector3 worldPosition = terrainPosition + new Vector3(xPosition * terrainSize.x, 0, yPosition * terrainSize.z);

                    heightMap[x, y] = terrainObject.SampleHeight(worldPosition) - terrainPosition.y;
                }
            }

            return heightMap;
        }
        private Vector3[,] CalculateNormalMap(float[,] heightMap, Vector3 terrainSize, int terrainResolution)
        {
            Vector3[,] normalMap = new Vector3[terrainResolution, terrainResolution];
            float xStep = terrainSize.x / (terrainResolution - 1);
            float zStep = terrainSize.z / (terrainResolution - 1);

            for (int y = 0; y < terrainResolution; y++)
            {
                for (int x = 0; x < terrainResolution; x++)
                {
                    // Get neighboring heights (with edge clamping)
                    int x1 = Mathf.Max(x - 1, 0);
                    int x2 = Mathf.Min(x + 1, terrainResolution - 1);
                    int y1 = Mathf.Max(y - 1, 0);
                    int y2 = Mathf.Min(y + 1, terrainResolution - 1);

                    // Calculate surface normal using central differences
                    float heightL = heightMap[x1, y];
                    float heightR = heightMap[x2, y];
                    float heightD = heightMap[x, y1];
                    float heightU = heightMap[x, y2];

                    Vector3 normal = new Vector3((heightL - heightR) / (2f * xStep), 1f, (heightD - heightU) / (2f * zStep)).normalized;

                    normalMap[x, y] = normal;
                }
            }

            return normalMap;
        }
    }
}