using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using TreeEditor;

namespace Core.Graphics.Editor
{
    public static class EditorUtility
    {
        [MenuItem("Tools/Create Texture2D Array", true, 15)]
        private static bool ValidateCreateTextureArray() => Selection.objects.OfType<Texture2D>().Count() >= 2;
        [MenuItem("Tools/Create Texture2D Array", false, 15)]
        private static void CreateTextureArray()
        {
            Texture2D[] textures = Selection.objects.OfType<Texture2D>().OrderBy(t => t.name).ToArray();

            if (textures.Length == 0)
            {
                Debug.LogError("Create Texture2D Array failed! Selected textures is null!");
                return;
            }

            Texture2D first = textures[0];
            TextureImporter firstImporter = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(first));

            bool isLinear = !firstImporter.sRGBTexture;

            Texture2DArray array = new(first.width, first.height, textures.Length, first.format, true, isLinear)
            {
                filterMode = first.filterMode,
                wrapMode = first.wrapMode
            };

            for (int i = 0; i < textures.Length; i++)
            {
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(textures[i]));

                if (importer.textureType != firstImporter.textureType)
                {
                    Debug.LogError("All textures must have same Texture Type.");
                    return;
                }

                if (importer.sRGBTexture != firstImporter.sRGBTexture)
                {
                    Debug.LogError("All textures must use same Color Space.");
                    return;
                }

                for (int mip = 0; mip < textures[i].mipmapCount; mip++)
                {
                    UnityEngine.Graphics.CopyTexture(textures[i], 0, mip, array, i, mip);
                }
            }

            array.Apply(false, false);

            string folderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(first));
            string savePath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folderPath, "NewTextureArray.asset"));

            AssetDatabase.CreateAsset(array, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Texture2D Array created at: {savePath} ({textures.Length} slice)");
        }

        [MenuItem("Tools/Create Texture2D Atlas", true, 16)]
        private static bool ValidateCreateTextureAtlas() => Selection.objects.OfType<Texture2D>().Count() >= 2;
        [MenuItem("Tools/Create Texture2D Atlas", false, 16)]
        private static void CreateTextureAtlas()
        {
            Texture2D[] textures = Selection.objects.OfType<Texture2D>().OrderBy(t => t.name).ToArray();
            Texture2D first = textures[0];
            TextureImporter firstImporter = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(first));

            bool isLinear = !firstImporter.sRGBTexture;

            if (textures.Length == 0) 
            { 
                Debug.LogError("Create Texture2D Atlas failed! Selected textures is null!"); 
                return;
            }

            static Vector2Int calculateGrid(int count)
            {
                int cols = Mathf.CeilToInt(Mathf.Sqrt(count));
                int rows = Mathf.CeilToInt(count / (float)cols);

                return new(cols, rows);
            }

            Vector2Int grid = calculateGrid(textures.Length);
            int cols = grid.x;
            int rows = grid.y;

            // Hücre boyutu: en büyük texture'a göre standardize et
            int cellWidth = textures.Max(t => t.width);
            int cellHeight = textures.Max(t => t.height);

            int atlasWidth = cellWidth * cols;
            int atlasHeight = cellHeight * rows;

            RenderTextureDescriptor desc = new(atlasWidth, atlasHeight, RenderTextureFormat.ARGB32, 0)
            {
                sRGB = !isLinear
            };

            RenderTexture rt = new(desc);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear);

            // UV rect bilgisi topla (shader'da kullanmak için)
            Rect[] uvRects = new Rect[textures.Length];

            for (int i = 0; i < textures.Length; i++)
            {
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(textures[i]));

                if (importer.textureType != firstImporter.textureType)
                {
                    Debug.LogError("All textures must have same Texture Type.");
                    return;
                }

                if (importer.sRGBTexture != firstImporter.sRGBTexture)
                {
                    Debug.LogError("All textures must use same Color Space.");
                    return;
                }

                int col = i % cols;
                int row = i / cols;

                int flippedRow = rows - 1 - row;

                Rect pixelRect = new(col * cellWidth, flippedRow * cellHeight, cellWidth, cellHeight);
                UnityEngine.Graphics.Blit(textures[i], rt, new Vector2(1, 1), new Vector2(0, 0));

                UnityEngine.Graphics.SetRenderTarget(rt);
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, atlasWidth, atlasHeight, 0);
                UnityEngine.Graphics.DrawTexture(new Rect(pixelRect.x, atlasHeight - pixelRect.y - cellHeight, cellWidth, cellHeight), textures[i]);
                GL.PopMatrix();

                uvRects[i] = new
                (
                    (float)(col * cellWidth) / atlasWidth,
                    (float)(flippedRow * cellHeight) / atlasHeight,
                    (float)cellWidth / atlasWidth,
                    (float)cellHeight / atlasHeight
                );
            }

            Texture2D atlasTex = new(atlasWidth, atlasHeight, TextureFormat.RGBA32, true);
            atlasTex.ReadPixels(new Rect(0, 0, atlasWidth, atlasHeight), 0, 0);
            atlasTex.Apply();

            RenderTexture.active = prev;
            rt.Release();
            Object.DestroyImmediate(rt);

            string folderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(first));
            string pngPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folderPath, "NewTextureAtlas.png"));

            File.WriteAllBytes(pngPath, atlasTex.EncodeToPNG());
            AssetDatabase.Refresh();

            Debug.Log($"Texture2D Atlas created at: {pngPath} (Grid: {cols}x{rows})");
        }

        [MenuItem("Tools/Bake Mesh to FBX", true, 17)]
        private static bool ValidateBakeMeshToFBX() => Selection.activeObject is Mesh;
        [MenuItem("Tools/Bake Mesh to FBX", false, 17)]
        private static void BakeMeshToFBX()
        {
            Mesh mesh = Selection.activeObject as Mesh;

            if (mesh == null)
            {
                Debug.LogWarning("Bake mesh to FBX failed! selected object is not valid!");
                return;
            }

            GameObject temp = new(mesh.name);
            temp.AddComponent<MeshFilter>().sharedMesh = mesh;
            temp.AddComponent<MeshRenderer>();

            string assetPath = AssetDatabase.GetAssetPath(mesh);
            string fbxPath = Path.Combine(Path.GetDirectoryName(assetPath), mesh.name + ".fbx");

            ModelExporter.ExportObject(fbxPath, temp);

            Object.DestroyImmediate(temp);
            AssetDatabase.Refresh();

            Debug.Log($"Bake mesh to FBX successfull! Path: {fbxPath}");
        }

        [MenuItem("Tools/Bake Object to FBX", true, 18)]
        private static bool ValidateBakeObjectToFBX() => Selection.activeGameObject != null;
        [MenuItem("Tools/Bake Object to FBX", false, 18)]
        private static void BakeObjectToFBX()
        {
            GameObject source = Selection.activeGameObject;

            if (source == null)
            {
                Debug.LogWarning("Bake object to FBX failed! selected object is not valid!");
                return;
            }

            MeshFilter[] meshFilters = source.GetComponentsInChildren<MeshFilter>();

            if (meshFilters.Length == 0)
            {
                return;
            }

            List<Material> allMaterials = new();
            Dictionary<Material, List<CombineInstance>> materialGroups = new();

            foreach (MeshFilter meshFilter in meshFilters)
            {
                Renderer renderer = meshFilter.GetComponent<Renderer>();

                if (renderer == null || meshFilter.sharedMesh == null)
                {
                    continue;
                }

                Material[] materials = renderer.sharedMaterials;
                Mesh mesh = meshFilter.sharedMesh;

                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    Material material = i < materials.Length ? materials[i] : materials[^1];

                    if (material == null)
                    {
                        continue;
                    }

                    if (!materialGroups.ContainsKey(material))
                    {
                        materialGroups[material] = new();
                    }
  
                    materialGroups[material].Add
                    (
                        new()
                        {
                            mesh = mesh,
                            subMeshIndex = i,
                            transform = meshFilter.transform.localToWorldMatrix
                        }
                    );
                }
            }

            if (materialGroups.Count == 0)
            {
                return;
            }

            List<CombineInstance> combinedInstances = new();

            foreach (var kvp in materialGroups)
            {
                Mesh groupMesh = new();
                groupMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                groupMesh.CombineMeshes(kvp.Value.ToArray(), true, true); // mergeSubMeshes = true

                combinedInstances.Add
                (
                    new() 
                    { 
                        mesh = groupMesh, 
                        subMeshIndex = 0, 
                        transform = Matrix4x4.identity 
                    }
                );

                allMaterials.Add(kvp.Key);
            }

            Mesh finalMesh = new();
            finalMesh.name = source.name + "_Combined";
            finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            finalMesh.CombineMeshes(combinedInstances.ToArray(), false, false);

            GameObject tempGO = new(source.name + "_Combined");
            MeshFilter tempFilter = tempGO.AddComponent<MeshFilter>();
            MeshRenderer tempRenderer = tempGO.AddComponent<MeshRenderer>();

            tempFilter.sharedMesh = finalMesh;
            tempRenderer.sharedMaterials = allMaterials.ToArray();

            string folder = "Assets";
            string path = AssetDatabase.GetAssetPath(source);

            if (!string.IsNullOrEmpty(path))
            {
                folder = Path.GetDirectoryName(path);
            }

            string folderPath = Path.Combine(folder, source.name + "_Combined.fbx");
            string fbxPath = AssetDatabase.GenerateUniqueAssetPath(folderPath);

            ModelExporter.ExportObject(fbxPath, tempGO);

            Object.DestroyImmediate(tempGO);

            AssetDatabase.Refresh();

            Debug.Log($"Bake object to FBX successfull! Path: {fbxPath} << Material Count: {allMaterials.Count} << Vertex Count: {finalMesh.vertexCount}");
        }

        [MenuItem("Tools/Bake Tree to FBX", true, 19)]
        private static bool ValidateBakeTreeToFBX() => Selection.activeGameObject != null;
        [MenuItem("Tools/Bake Tree to FBX", false, 19)]
        private static void BakeTreeToFBX()
        {
            GameObject source = Selection.activeGameObject;

            if (source == null)
            {
                return;
            }

            Tree treeComponent = source.GetComponent<Tree>();

            if (treeComponent == null || treeComponent.data == null)
            {
                Debug.LogWarning("Bake tree to FBX failed! selected object is not valid!");
                return;
            }

            TreeData treeData = treeComponent.data as TreeData;

            if (treeData == null)
            {
                return;
            }

            List<TreeMaterial> treeMaterials = new();
            List<TreeVertex> treeVertices = new();
            List<TreeTriangle> treeTriangles = new();
            List<TreeAOSphere> treeSpheres = new();
            TreeGroupRoot treeRoot = treeData.root;

            int buildFlags = 0;

            if (treeRoot.enableAmbientOcclusion)
            {
                buildFlags |= (int)TreeGroup.BuildFlag.BuildAmbientOcclusion;
            }

            if (treeRoot.enableWelding)
            {
                buildFlags |= (int)TreeGroup.BuildFlag.BuildWeldParts;
            }

            treeData.UpdateMesh(source.transform.worldToLocalMatrix, treeMaterials, treeVertices, treeTriangles, treeSpheres, buildFlags, treeRoot.adaptiveLODQuality, treeRoot.aoDensity);

            if (treeVertices.Count == 0 || treeTriangles.Count == 0 || treeMaterials.Count == 0)
            {
                Debug.LogWarning("Bake tree to FBX failed! Branch/Leaf groups does not have material or groups does not have any vertices!?");
                return;
            }

            if (treeVertices.Count > 65000)
            {
                Debug.LogWarning("Bake tree to FBX failed! Vertex count is higher than 65000");
                return;
            }

            Vector3[] tmpPos = new Vector3[treeVertices.Count];
            Vector3[] tmpNor = new Vector3[treeVertices.Count];
            Vector2[] tmpUV0 = new Vector2[treeVertices.Count];
            Vector2[] tmpUV1 = new Vector2[treeVertices.Count];
            Vector4[] tmpTan = new Vector4[treeVertices.Count];
            Color[] tmpCol = new Color[treeVertices.Count];

            for (int i = 0; i < treeVertices.Count; i++)
            {
                tmpPos[i] = treeVertices[i].pos;
                tmpNor[i] = treeVertices[i].nor;
                tmpUV0[i] = treeVertices[i].uv0;
                tmpUV1[i] = treeVertices[i].uv1;
                tmpTan[i] = treeVertices[i].tangent;
                tmpCol[i] = treeVertices[i].color;
            }

            Mesh bakedMesh = new()
            {
                indexFormat         = UnityEngine.Rendering.IndexFormat.UInt32,
                vertices            = tmpPos,
                normals             = tmpNor,
                uv                  = tmpUV0,
                uv2                 = tmpUV1,
                tangents            = tmpTan,
                colors              = tmpCol,
                subMeshCount        = treeMaterials.Count
            };

            List<Material> bakedMaterials = new(treeMaterials.Count);

            for (int i = 0; i < treeMaterials.Count; i++)
            {
                List<int> triangleIndices = new();

                for (int j = 0; j < treeTriangles.Count; j++)
                {
                    if (treeTriangles[j].materialIndex == i)
                    {
                        triangleIndices.Add(treeTriangles[j].v[0]);
                        triangleIndices.Add(treeTriangles[j].v[1]);
                        triangleIndices.Add(treeTriangles[j].v[2]);
                    }
                }

                bakedMesh.SetTriangles(triangleIndices, i);
                bakedMaterials.Add(treeMaterials[i].material);
            }

            bakedMesh.RecalculateBounds();
           
            const string ROOT_FOLDER_NAME = "Assets";
            const string TREE_FOLDER_NAME = "BakedTrees";
            const string TREE_FOLDER_PATH = ROOT_FOLDER_NAME + "/" + TREE_FOLDER_NAME;

            string treeName = source.name;
            string treeFolder = $"{TREE_FOLDER_PATH}/{treeName}";

            if (!AssetDatabase.IsValidFolder(TREE_FOLDER_PATH))
            {
                AssetDatabase.CreateFolder(ROOT_FOLDER_NAME, TREE_FOLDER_NAME);
            }

            if (!AssetDatabase.IsValidFolder(treeFolder))
            {
                AssetDatabase.CreateFolder(TREE_FOLDER_PATH, treeName);
            }

            string prefabPath = $"{treeFolder}/{treeName}.prefab";
            string fbxPath = $"{treeFolder}/{treeName}.fbx";
            string meshPath = $"{treeFolder}/{treeName}.mesh";

            if (AssetDatabase.LoadAssetAtPath<Object>(prefabPath) != null) AssetDatabase.DeleteAsset(prefabPath);
            if (AssetDatabase.LoadAssetAtPath<Object>(fbxPath) != null) AssetDatabase.DeleteAsset(fbxPath);
            if (AssetDatabase.LoadAssetAtPath<Object>(meshPath) != null) AssetDatabase.DeleteAsset(meshPath);

            bakedMesh.name = treeName;

            AssetDatabase.CreateAsset(bakedMesh, meshPath);

            GameObject bakedRoot = new(treeName);
            bakedRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            bakedRoot.transform.localScale = Vector3.one;

            MeshFilter bakedFilter = bakedRoot.AddComponent<MeshFilter>();
            MeshRenderer bakedRenderer = bakedRoot.AddComponent<MeshRenderer>();
            bakedFilter.sharedMesh = bakedMesh;
            bakedRenderer.sharedMaterials = bakedMaterials.ToArray();

            AssetDatabase.SaveAssets();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bakedRoot, prefabPath);
            string absoluteFbxPath = Path.GetFullPath(fbxPath);
            string exportedFbxPath = ModelExporter.ExportObject(absoluteFbxPath, bakedRoot);

            Object.DestroyImmediate(bakedRoot);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            Debug.Log($"Bake tree to FBX successfull! Path: {exportedFbxPath}");
        }
    }
}