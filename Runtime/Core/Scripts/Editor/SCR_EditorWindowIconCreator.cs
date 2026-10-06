using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    internal sealed class EditorWindowIconCreator : EditorWindow
    {
        private const int MAX_ICONS_PER_ROW = 16;

        [SerializeField] private GameObject root;

        [SerializeField, Min(0)] private int iconLayer = 5;
        [SerializeField] private int iconSize = 64;
        [SerializeField] private int iconPadding = 4;

        [SerializeField] private Vector3 cameraRotation = new(20f, -135f, 0f);
        [SerializeField] private float cameraPadding = 1.1f;

        [MenuItem("Tools/Icon Creator")]
        private static void ShowWindow() => GetWindow<EditorWindowIconCreator>("Icon Creator");

        private void OnGUI()
        {
            root = (GameObject)EditorGUILayout.ObjectField("Root", root, typeof(GameObject), true);
            iconSize = Mathf.Max(1, EditorGUILayout.IntField("Icon Size", iconSize));
            iconPadding = Mathf.Max(0, EditorGUILayout.IntField("Padding", iconPadding));
            cameraRotation = EditorGUILayout.Vector3Field("Camera Rotation", cameraRotation);
            cameraPadding = Mathf.Max(1f, EditorGUILayout.FloatField("Camera Padding", cameraPadding));

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(root == null))
            {
                if (GUILayout.Button("Export Atlas"))
                {
                    ExportAtlas();
                }
            }
        }

        private void ExportAtlas()
        {
            List<GameObject> items = GetItems();

            if (items.Count == 0)
            {
                Debug.LogWarning("Icon Creator failed! No items found.");
                return;
            }

            string path = UnityEditor.EditorUtility.SaveFilePanel("Export Item Icon Atlas", Application.dataPath, "ItemIconAtlas", "png");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            int columns = Mathf.Min(items.Count, MAX_ICONS_PER_ROW);
            int rows = Mathf.CeilToInt(items.Count / (float)MAX_ICONS_PER_ROW);

            int atlasWidth = columns * iconSize + (columns - 1) * iconPadding;
            int atlasHeight = rows * iconSize + (rows - 1) * iconPadding;

            Texture2D atlas = new(atlasWidth, atlasHeight, TextureFormat.RGBA32, false);
            ClearTexture(atlas);

            GameObject cameraObject = new("Item Icon Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            camera.orthographic = true;
            camera.enabled = false;
            camera.cullingMask = 1 << iconLayer;
            camera.allowHDR = false;
            camera.allowMSAA = false;

            RenderTexture renderTexture = new(iconSize, iconSize, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            renderTexture.useMipMap = false;
            renderTexture.autoGenerateMips = false;
            renderTexture.Create();

            camera.targetTexture = renderTexture;

            for (int i = 0; i < items.Count; i++)
            {
                GameObject preview = Instantiate(items[i]);

                preview.name = items[i].name + "_Preview";
                preview.SetLayer(iconLayer, true);

                if (!TryGetBounds(preview, out Bounds bounds))
                {
                    DestroyImmediate(preview);
                    continue;
                }

                FrameCamera(camera, bounds);

                Texture2D black = RenderIcon(camera, renderTexture, Color.black);
                Texture2D white = RenderIcon(camera, renderTexture, Color.white);
                Texture2D icon = ReconstructAlpha(black, white);

                DestroyImmediate(black);
                DestroyImmediate(white);

                int column = i % columns;
                int row = i / columns;

                int x = column * (iconSize + iconPadding);
                int y = atlasHeight - iconSize - row * (iconSize + iconPadding);

                atlas.SetPixels(x, y, iconSize, iconSize, icon.GetPixels());

                DestroyImmediate(icon);
                DestroyImmediate(preview);
            }

            atlas.Apply();
            File.WriteAllBytes(path, atlas.EncodeToPNG());

            camera.targetTexture = null;
            renderTexture.Release();

            DestroyImmediate(renderTexture);
            DestroyImmediate(cameraObject);
            DestroyImmediate(atlas);

            AssetDatabase.Refresh();

            Debug.Log($"Item icon atlas exported: {path}");
        }

        private Texture2D RenderIcon(Camera camera, RenderTexture renderTexture, Color backgroundColor)
        {
            camera.backgroundColor = backgroundColor;
            camera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;

            Texture2D texture = new(iconSize, iconSize, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, iconSize, iconSize), 0, 0);
            texture.Apply();

            RenderTexture.active = previous;

            return texture;
        }
        private Texture2D ReconstructAlpha(Texture2D black, Texture2D white)
        {
            Color[] blackPixels = black.GetPixels();
            Color[] whitePixels = white.GetPixels();
            Color[] pixels = new Color[blackPixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                Color b = blackPixels[i];
                Color w = whitePixels[i];

                float alpha = 1f - ((w.r - b.r + w.g - b.g + w.b - b.b) / 3f);
                alpha = Mathf.Clamp01(alpha);

                if (alpha > 0.0001f)
                {
                    pixels[i] = new Color(b.r / alpha, b.g / alpha, b.b / alpha, alpha);
                }
                else
                {
                    pixels[i] = Color.clear;
                }
            }

            Texture2D result = new(iconSize, iconSize, TextureFormat.RGBA32, false);
            result.SetPixels(pixels);
            result.Apply();

            return result;
        }

        private List<GameObject> GetItems()
        {
            List<GameObject> items = new();

            if (root == null)
            {
                return items;
            }

            for (int i = 0; i < root.transform.childCount; i++)
            {
                GameObject item = root.transform.GetChild(i).gameObject;

                if (item.activeInHierarchy)
                {
                    items.Add(item);
                }
            }

            return items;
        }
        private static bool TryGetBounds(GameObject item, out Bounds bounds)
        {
            Renderer[] renderers = item.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
            {
                bounds = default;
                return false;
            }

            bounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return true;
        }

        private void FrameCamera(Camera camera, Bounds bounds)
        {
            camera.transform.rotation = Quaternion.Euler(cameraRotation);

            Vector3 cameraRight = camera.transform.right;
            Vector3 cameraUp = camera.transform.up;
            Vector3 extents = bounds.extents;

            float horizontalSize = Mathf.Abs(cameraRight.x) * extents.x + Mathf.Abs(cameraRight.y) * extents.y + Mathf.Abs(cameraRight.z) * extents.z;
            float verticalSize = Mathf.Abs(cameraUp.x) * extents.x + Mathf.Abs(cameraUp.y) * extents.y + Mathf.Abs(cameraUp.z) * extents.z;

            camera.orthographicSize = Mathf.Max(verticalSize, horizontalSize) * cameraPadding;
            camera.transform.position = bounds.center - camera.transform.forward * (bounds.extents.magnitude + 10f);
        }
        private static void ClearTexture(Texture2D texture)
        {
            Color[] pixels = new Color[texture.width * texture.height];

            texture.SetPixels(pixels);
            texture.Apply();
        }
    }
}