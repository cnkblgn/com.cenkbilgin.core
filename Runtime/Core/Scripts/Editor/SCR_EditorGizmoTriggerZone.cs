using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    [InitializeOnLoad]
    public static class EditorGizmoTriggerZone
    {
        private static readonly Mesh Mesh;
        private static readonly Material SelectedMaterial;
        private static readonly Material DefaultMaterial;
        private static readonly Shader Shader;

        private const string SHADER_ID = "Hidden/FX_UnlitTrigger";

        static EditorGizmoTriggerZone()
        {
            Mesh = CreateCubeMesh();
            Shader = Shader.Find(SHADER_ID);

            if (Shader != null)
            {
                DefaultMaterial = new Material(Shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                SelectedMaterial = new Material(Shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                DefaultMaterial.color = Color.yellow;
                SelectedMaterial.color = Color.red;
            }
        }

        private static Mesh CreateCubeMesh()
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            try
            {
                return cube.GetComponent<MeshFilter>().sharedMesh;
            }
            finally
            {
                Object.DestroyImmediate(cube);
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void Draw(TriggerZone zone, GizmoType gizmoType)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (Shader == null)
            {
                Debug.LogWarning($"Missing shader with id: {SHADER_ID}");
                return;
            }

            bool selected = (gizmoType & GizmoType.Selected) != 0;
            Material material = selected ? SelectedMaterial : DefaultMaterial;

            Matrix4x4 matrix = Matrix4x4.TRS(zone.transform.TransformPoint(zone.GetCenter()), zone.transform.rotation, Vector3.Scale(zone.GetSize(), zone.transform.lossyScale));

            material.SetPass(0);

            Graphics.DrawMeshNow(Mesh, matrix);

            Handles.color = selected ? Color.red : Color.yellow;

            using (new Handles.DrawingScope(matrix))
            {
                Handles.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }
    }
}
