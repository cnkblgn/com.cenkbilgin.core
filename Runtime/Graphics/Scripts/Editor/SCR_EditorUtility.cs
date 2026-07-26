using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;

namespace Core.Graphics.Editor
{
    public static class EditorUtility
    {
        [MenuItem("Tools/Create Texture2D Array", true, 15)]
        private static bool ValidateCreateTextureArray() => Selection.objects.OfType<Texture2D>().Count() >= 2;
        [MenuItem("Tools/Create Texture2D Array", false, 15)]
        private static void CreateArray()
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
        private static bool ValidateCreateAtlas() => Selection.objects.OfType<Texture2D>().Count() >= 2;
        [MenuItem("Tools/Create Texture2D Atlas", false, 16)]
        private static void CreateAtlas()
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
    }
}