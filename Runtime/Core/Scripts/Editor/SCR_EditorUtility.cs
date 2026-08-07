using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    using static CoreUtility;

    public static class EditorUtility
    {
        private const int SIM_STEP_COUNT = 300;
        private const float SIM_STEP_SECOND = 0.02f;

        private static GameObject copiedObject;

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
        
        [MenuItem("Tools/Copy All Components %#c", false, 1)]
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

        [MenuItem("Tools/Copy All Components %#c", true, 1)]
        private static bool ValidateCopy() => Selection.activeGameObject != null;

        [MenuItem("Tools/Paste All Components %#v", false, 2)]
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
            
        [MenuItem("Tools/Paste All Components %#v", true, 2)]
        private static bool ValidatePaste() => copiedObject != null && Selection.activeGameObject != null;

        [MenuItem("Tools/Reset Transform %#r", false, 3)]
        private static void ResetTransform()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                Undo.RecordObject(go.transform, "Reset Transform");

                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                UnityEditor.EditorUtility.SetDirty(go.transform);
            }
        }

        [MenuItem("Tools/Reset Transform %#r", true, 3)]
        private static bool ValidateResetTransform() => !EditorApplication.isPlaying && Selection.gameObjects.Length > 0;

        [MenuItem("Tools/Simulate Transforms %#e", false, 4)]
        private static void SimulateTransform()
        {
            GameObject[] selected = Selection.gameObjects;
            if (selected.Length == 0)
            {
                Debug.LogWarning("Simulate transform failed! No selected game object.");
                return;
            }

            Undo.SetCurrentGroupName("Simulate Transforms");
            int undoGroup = Undo.GetCurrentGroup();

            var tempRigidbodies = new List<Rigidbody>();
            var tempColliders = new List<Collider>();

            foreach (GameObject go in selected)
            {
                Undo.RecordObject(go.transform, "Simulate Transforms");

                if (!go.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb = Undo.AddComponent<Rigidbody>(go);
                    tempRigidbodies.Add(rb);
                }

                if (go.GetComponent<Collider>() == null)
                {
                    Collider col = Undo.AddComponent<BoxCollider>(go);
                    tempColliders.Add(col);
                }

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            SimulationMode previousMode = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;

            for (int i = 0; i < SIM_STEP_COUNT; i++)
            {
                Physics.Simulate(SIM_STEP_SECOND);
            }

            Physics.simulationMode = previousMode;

            foreach (Rigidbody rb in tempRigidbodies)
            {
                if (rb != null) Undo.DestroyObjectImmediate(rb);
            }
            foreach (Collider col in tempColliders)
            {
                if (col != null) Undo.DestroyObjectImmediate(col);
            }

            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log($"Simulate transform successfull! Total simulated: {selected.Length}");
        }

        [MenuItem("Tools/Simulate Transforms %#e", true, 4)]
        private static bool ValidateSimulateTransform() => !EditorApplication.isPlaying && Selection.gameObjects.Length > 0;

        [MenuItem("Tools/Snap Transform %#t", false, 5)]
        private static void SnapTransform()
        {
            GameObject[] selected = Selection.gameObjects;
            if (selected.Length == 0)
            {
                Debug.LogWarning("Snap transform failed! No selected game object.");
                return;
            }

            Undo.SetCurrentGroupName("Snap Transforms");
            int undoGroup = Undo.GetCurrentGroup();
            int snappedCount = 0;

            foreach (GameObject go in selected)
            {
                Vector3 origin = go.transform.position + Vector3.up * 1024;
                RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 4096);

                RaycastHit? closestHit = null;
                float closestDistance = float.MaxValue;

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.transform.IsChildOf(go.transform))
                    {
                        continue;
                    }

                    if (hit.distance < closestDistance)
                    {
                        closestDistance = hit.distance;
                        closestHit = hit;
                    }
                }

                if (!closestHit.HasValue)
                {
                    Debug.LogWarning($"Snap transform failed! '{go.name}' has no viable collider to snap!", go);
                    continue;
                }

                RaycastHit groundHit = closestHit.Value;

                Undo.RecordObject(go.transform, "Snap Transform");

                go.transform.SnapToGround(groundHit.point, groundHit.normal);

                snappedCount++;
            }

            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log($"Snap transform successfull! Total snapped: {snappedCount}/{selected.Length}");
        }

        [MenuItem("Tools/Snap Transform %#t", true, 5)]
        private static bool ValidateSnapTransform() => !EditorApplication.isPlaying && Selection.gameObjects.Length > 0;

        [MenuItem("Tools/Search and Remap Materials", false, 6)]
        private static void SearchAndRemapMaterials()
        {
            UnityEngine.Object[] objects = Selection.objects;

            const string formatFbx = ".fbx";
            const string formatObj = ".obj";

            if (objects.Length <= 0)
            {
                Debug.LogWarning($"Please select [{formatFbx}] asset or [{formatObj}] asset");
                return;
            }

            foreach (UnityEngine.Object obj in objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                if (!string.Equals(Path.GetExtension(path), formatFbx, StringComparison.OrdinalIgnoreCase) && !string.Equals(Path.GetExtension(path), formatObj, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (AssetImporter.GetAtPath(path) is not ModelImporter importer)
                {
                    continue;
                }

                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                importer.SearchAndRemapMaterials(
                    ModelImporterMaterialName.BasedOnMaterialName,
                    ModelImporterMaterialSearch.Everywhere);

                importer.SaveAndReimport();

                Debug.Log($"Material found and remapped to: {path}");
            }
        }

        [MenuItem("Tools/Search and Remap Materials", true, 6)]
        private static bool ValidateSearchAndRemapMaterials() => !EditorApplication.isPlaying && Selection.gameObjects.Length > 0;

        [MenuItem("Tools/Search and Remove Missing Components", false, 7)]
        private static void SearchAndRemoveMissingComponents()
        {
            int totalRemoved = 0;

            foreach (UnityEngine.Object obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                // Prefab Asset
                if (!string.IsNullOrEmpty(path))
                {
                    GameObject prefab = PrefabUtility.LoadPrefabContents(path);

                    foreach (Transform t in prefab.GetComponentsInChildren<Transform>(true))
                    {
                        totalRemoved += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                    }

                    PrefabUtility.SaveAsPrefabAsset(prefab, path);
                    PrefabUtility.UnloadPrefabContents(prefab);
                }
                // Scene Object
                else if (obj is GameObject go)
                {
                    foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
                    {
                        Undo.RegisterCompleteObjectUndo(t.gameObject, "Remove Missing Components");
                        totalRemoved += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                        UnityEditor.EditorUtility.SetDirty(t.gameObject);
                    }
                }
            }

            AssetDatabase.SaveAssets();

            Debug.Log($"Removed {totalRemoved} missing component(s).");
        }

        [MenuItem("Tools/Search and Remove Missing Components", true, 7)]
        private static bool ValidateSearchAndRemoveMissingComponents() => !EditorApplication.isPlaying && Selection.gameObjects.Length > 0;

        public static void DrawButton(string name, UnityEngine.Object target, Action action)
        {
            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (GUILayout.Button(name))
                {
                    Undo.RecordObject(target, name);

                    action?.Invoke();

                    UnityEditor.EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        public static void DrawArrow(Vector3 position, Vector3 direction, float length = 0.25f, float angle = 20.0f)
        {
            Gizmos.DrawRay(position, direction);
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + angle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - angle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(position + direction, right * length);
            Gizmos.DrawRay(position + direction, left * length);
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