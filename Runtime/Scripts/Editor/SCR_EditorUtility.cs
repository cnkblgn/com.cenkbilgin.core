using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    using static CoreUtility;

    public static class EditorUtility
    {
        [MenuItem("Tools/Find Missing Scripts", priority = 0)]
        public static void FindMissingScripts()
        {
            foreach (GameObject gameObject in GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
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

        [MenuItem("Tools/Create Prefab From Object With Parent", false, 1)]
        public static void CreatePrefab()
        {
            string prefabsFolder = "Assets/";

            if (!AssetDatabase.IsValidFolder(prefabsFolder))
            {
                string parentFolder = Path.GetDirectoryName(prefabsFolder);
                string newFolder = Path.GetFileName(prefabsFolder);
                AssetDatabase.CreateFolder(parentFolder, newFolder);
            }

            foreach (GameObject selectedObject in Selection.gameObjects.Where(go => go.name.StartsWith("MDL_")))
            {
                CreatePrefabInternal(selectedObject, prefabsFolder);
            }

            AssetDatabase.Refresh();
        }
        private static void CreatePrefabInternal(GameObject originalObject, string folderPath)
        {
            string originalName = originalObject.name;
            string prefabName = originalName.Replace("MDL_", "PFB_");
            string prefabPath = Path.Combine(folderPath, $"{prefabName}.prefab").Replace("\\", "/");

            // Delete existing prefab if it exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            GameObject parentObject = new(prefabName);
            originalObject.transform.SetParent(parentObject.transform);
            originalObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            PrefabUtility.SaveAsPrefabAsset(parentObject, prefabPath);

            // Replace original with prefab instance
            GameObject newInstance = PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)) as GameObject;

            Object.DestroyImmediate(parentObject);
        }

        [MenuItem("Tools/Snap Selected To Decimal", false, 2)]
        private static void SnapSelectedDecimal()
        {
            //float snap(float value) => Mathf.Floor(value * 10f) / 10f;
            static float snap(float value)
            {
                // 0.25 aralýkla yuvarlamak için:
                float lower = Mathf.Floor(value); // en yakýn alt tam sayý
                float fraction = value - lower;

                if (fraction < 0.25f)
                    return lower;
                else if (fraction < 0.75f)
                    return lower + 0.5f;
                else
                    return lower + 1f;
            }

            foreach (GameObject obj in Selection.gameObjects)
            {
                Undo.RecordObject(obj.transform, "Snap To Int");

                // Snap position
                Vector3 pos = obj.transform.localPosition;
                pos.x = snap(pos.x);
                pos.y = snap(pos.y);
                pos.z = snap(pos.z);
                obj.transform.localPosition = pos;

                // Snap rotation
                Vector3 rot = obj.transform.localEulerAngles;
                rot.x = snap(rot.x);
                rot.y = snap(rot.y);
                rot.z = snap(rot.z);
                obj.transform.localEulerAngles = rot;
            }
        }
        [MenuItem("Tools/Snap Selected To Decimal", true)]
        private static bool SnapSelectedDecimalValidate() => Selection.gameObjects.Length > 0;

        [MenuItem("Tools/Snap Selected To Int %#i", false, 3)] // Ctrl+Shift+I (Windows) / Cmd+Shift+I (Mac)
        private static void SnapSelected()
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                Undo.RecordObject(obj.transform, "Snap To Int");

                // Snap position
                Vector3 pos = obj.transform.localPosition;
                pos.x = Mathf.Round(pos.x);
                pos.y = Mathf.Round(pos.y);
                pos.z = Mathf.Round(pos.z);
                obj.transform.localPosition = pos;

                // Snap rotation
                Vector3 rot = obj.transform.localEulerAngles;
                rot.x = Mathf.Round(rot.x);
                rot.y = Mathf.Round(rot.y);
                rot.z = Mathf.Round(rot.z);
                obj.transform.localEulerAngles = rot;
            }
        }
        [MenuItem("Tools/Snap Selected To Int %#i", true)]
        private static bool SnapSelectedValidate() => Selection.gameObjects.Length > 0;

        [MenuItem("Tools/Toggle Gizmos %g", false, 4)] // Ctrl+G or Cmd+G
        private static void ToggleGizmos()
        {
            if (SceneView.lastActiveSceneView == null)
            {
                return;
            }

            SceneView.lastActiveSceneView.drawGizmos = !SceneView.lastActiveSceneView.drawGizmos;
            SceneView.RepaintAll();
        }

        private static GameObject copiedObject;
        [MenuItem("Tools/Copy All Components", false, 5)]
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
        [MenuItem("Tools/Paste All Components", false, 6)]
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
        [MenuItem("Tools/Copy All Components", true)]
        private static bool ValidateCopy() => Selection.activeGameObject != null;
        [MenuItem("Tools/Paste All Components", true)]
        private static bool ValidatePaste() => copiedObject != null && Selection.activeGameObject != null;

        [MenuItem("Tools/Search and Remap Materials", false, 7)]
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

        public static T FindAssetByName<T>(string nameKeyword, string searchFolder = "Assets") where T : Object
        {
            // Construct the search query.
            // "nameKeyword" searches for assets that contain the keyword in the name.
            // "t:TypeName" is used to filter by type. For example, t:AudioSettingsConfig.
            string typeFilter = "t:" + typeof(T).Name;
            string searchQuery = nameKeyword + " " + typeFilter;

            // Search within the given folder (or the entire project)
            string[] guids = AssetDatabase.FindAssets(searchQuery, new[] { searchFolder });
            if (guids.Length == 0)
            {
                Debug.LogError("EditorUtility.FindAssetByName() No asset found with keyword: " + nameKeyword);
                return null;
            }

            // Get the first asset's path
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            return asset;
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