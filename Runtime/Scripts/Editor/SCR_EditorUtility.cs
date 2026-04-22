using System.IO;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    using static CoreUtility;

    public static class EditorUtility
    {
        [MenuItem("Tools/Toggle Gizmos %g", false, 0)] // Ctrl+G or Cmd+G
        private static void ToggleGizmos()
        {
            if (SceneView.lastActiveSceneView == null)
            {
                return;
            }

            SceneView.lastActiveSceneView.drawGizmos = !SceneView.lastActiveSceneView.drawGizmos;
            SceneView.RepaintAll();
        }
        [MenuItem("Tools/Validate All Components", priority = 1)]
        public static void ValidateComponents()
        {
            foreach (GameObject gameObject in GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            {
                foreach (Component component in gameObject.GetComponentsInChildren<Component>())
                {
                    if (component == null)
                    {
                        Debug.Log($"Missing script at {gameObject.name}", gameObject);
                        break;
                    }
                }
            }
        }
        private static GameObject copiedObject;
        [MenuItem("Tools/Copy All Components", false, 2)]
        private static void CopyComponents()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogWarning("No GameObject selected to copy from.");
                return;
            }

            copiedObject = Selection.activeGameObject;
            Debug.Log($"Copied components from: {copiedObject.name}");
        }
        [MenuItem("Tools/Paste All Components", false, 3)]
        private static void PasteComponents()
        {
            if (copiedObject == null)
            {
                Debug.LogWarning("No source GameObject copied yet!");
                return;
            }

            if (Selection.activeGameObject == null)
            {
                Debug.LogWarning("No target GameObject selected to paste into.");
                return;
            }

            var source = copiedObject;
            var target = Selection.activeGameObject;

            int copiedCount = 0;
            foreach (Component comp in source.GetComponents<Component>())
            {
                if (comp is Transform)
                    continue;

                System.Type type = comp.GetType();
                Component copy = target.AddComponent(type);

                // Copy all serialized properties
                SerializedObject srcSerialized = new SerializedObject(comp);
                SerializedObject dstSerialized = new SerializedObject(copy);
                SerializedProperty prop = srcSerialized.GetIterator();

                while (prop.NextVisible(true))
                {
                    dstSerialized.CopyFromSerializedProperty(prop);
                }

                dstSerialized.ApplyModifiedPropertiesWithoutUndo();
                copiedCount++;
            }

            copiedObject = null;
            Debug.Log($"Pasted {copiedCount} components from {source.name} to {target.name}");
        }
        [MenuItem("Tools/Copy All Components", true, 4)]
        private static bool ValidateCopy() => Selection.activeGameObject != null;
        [MenuItem("Tools/Paste All Components", true, 5)]
        private static bool ValidatePaste() => copiedObject != null && Selection.activeGameObject != null;
        [MenuItem("Tools/Search and Remap Materials", false, 6)]
        private static void SearchAndRemapMaterials()
        {
            Object[] objects = Selection.objects;

            if (objects.Length <= 0)
            {
                Debug.LogWarning("Please select .fbx asset");
                return;
            }

            foreach (Object obj in objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                if (!path.EndsWith(".fbx"))
                {
                    continue;
                }

                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;

                if (importer == null)
                {
                    continue;
                }

                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                importer.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.Everywhere);
                importer.SaveAndReimport();

                Debug.Log($"Material found and remapped to: {path}");
            }
        }

        public static T CreateAsset<T>(string assetPath) where T : ScriptableObject
        {
            // Klasör kýsmýný al
            string folderPath = Path.GetDirectoryName(assetPath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            T asset = ScriptableObject.CreateInstance<T>();

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);

            Debug.Log($"EditorUtility.CreateAsset() [{nameof(T)}] created at [{folderPath}]");
            return asset;
        }
        public static T FindAssetByKeyword<T>(string keyword, string folder = "Assets") where T : Object
        {
            string filter = "t:" + typeof(T).Name;
            string query = keyword + " " + filter;

            GUID[] guids = AssetDatabase.FindAssetGUIDs(query, new[] { folder });

            if (guids == null || guids.Length == 0)
            {
                Debug.LogError($"EditorUtility.FindAssetByKeyword() No asset found with keyword: " + keyword);
                return null;
            }

            return GetAssetFromGuid<T>(guids[0]);
        }
        public static T FindAssetByType<T>(string folder = "Assets") where T : Object
        {
            string filter = "t:" + typeof(T).Name;
            GUID[] guids = AssetDatabase.FindAssetGUIDs(filter, new[] { folder });

            if (guids.Length == 0)
            {
                Debug.LogError($"EditorUtility.FindAssetByName() No asset found with type: [{filter}]");
                return null;
            }

            return GetAssetFromGuid<T>(guids[0]);
        }
        public static T[] FindAssetsByType<T>(string folder = "Assets") where T : Object
        {
            string filter = "t:" + typeof(T).Name;
            GUID[] guids = AssetDatabase.FindAssetGUIDs(filter, new[] { folder });

            if (guids.Length == 0)
            {
                Debug.LogError($"EditorUtility.FindAssetByName() No asset found with type: [{filter}]");
                return null;
            }

            T[] assets = new T[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                assets[i] = GetAssetFromGuid<T>(guids[i]);
            }

            return assets;
        }
        private static T GetAssetFromGuid<T>(GUID guid) where T : Object
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset == null)
            {
                Debug.LogError($"EditorUtility.GetAssetFromGuid() No asset found with path: [{path}]");
            }

            return asset;
        }

        public static void DrawFieldOfView(Transform origin, float radius, float angle)
        {
            if (origin == null)
            {
                return;
            }

            Gizmos.color = COLOR_GREEN;
            Gizmos.DrawWireSphere(origin.position, radius);

            Vector3 left = new(Mathf.Sin((-angle / 2 + origin.eulerAngles.y) * Mathf.Deg2Rad), 0, Mathf.Cos((-angle / 2 + origin.eulerAngles.y) * Mathf.Deg2Rad));
            Vector3 right = new(Mathf.Sin((angle / 2 + origin.eulerAngles.y) * Mathf.Deg2Rad), 0, Mathf.Cos((angle / 2 + origin.eulerAngles.y) * Mathf.Deg2Rad));

            Gizmos.color = COLOR_BLUE;
            Gizmos.DrawLine(origin.position, origin.position + left * radius);
            Gizmos.DrawLine(origin.position, origin.position + right * radius);
        }
        public static void DrawCone(Transform origin, float radius, float angle)
        {
            if (!origin)
            {
                return;
            }

            Gizmos.color = COLOR_GREEN;
            Vector3 forward = origin.forward;
            Vector3 left = Quaternion.AngleAxis(-angle * 0.5f, origin.up) * forward;
            Vector3 right = Quaternion.AngleAxis(angle * 0.5f, origin.up) * forward;
            Gizmos.DrawWireSphere(origin.position, radius);

            Gizmos.color = COLOR_BLUE;
            Gizmos.DrawLine(origin.position, origin.position + left * radius);
            Gizmos.DrawLine(origin.position, origin.position + right * radius);
            Gizmos.DrawLine(origin.position, origin.position + forward * radius);
        }
        public static void DrawOutline(Rect rect, Color color, int thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }
        public static void DrawCapsule(Vector3 top, Vector3 bottom, float radius)
        {
            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawWireSphere(bottom, radius);

            Gizmos.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
            Gizmos.DrawLine(top - Vector3.forward * radius, bottom - Vector3.forward * radius);
            Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
            Gizmos.DrawLine(top - Vector3.right * radius, bottom - Vector3.right * radius);
        }
        public static void DrawCapsuleSweep(Vector3 top, Vector3 bottom, Vector3 direction, float radius, float distance)
        {
            DrawCapsule(top, bottom, radius);

            Vector3 t2 = top + direction * distance;
            Vector3 b2 = bottom + direction * distance;

            DrawCapsule(t2, b2, radius);

            Gizmos.DrawLine((top + bottom) * 0.5f, ((t2 + b2) * 0.5f));
        }
        public static void DrawCapsuleTarget(Vector3 top, Vector3 bottom, Vector3 direction, float radius, float distance)
        {
            Vector3 t2 = top + direction * distance;
            Vector3 b2 = bottom + direction * distance;

            DrawCapsule(t2, b2, radius);

            Gizmos.DrawLine((top + bottom) * 0.5f, ((t2 + b2) * 0.5f));
        }
    }
}