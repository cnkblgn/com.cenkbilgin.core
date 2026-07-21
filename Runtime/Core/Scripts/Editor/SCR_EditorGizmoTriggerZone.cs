using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Editor
{
    [InitializeOnLoad]
    public static class EditorGizmoTriggerZone
    {
        private static readonly Mesh Mesh;
        private static readonly Material Material;

        static EditorGizmoTriggerZone()
        {
            Mesh = CreateCubeMesh();
            Material = new(Shader.Find("Hidden/FX_TriggerZone")) { hideFlags = HideFlags.HideAndDontSave };
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

            Matrix4x4 matrix = Matrix4x4.TRS(zone.transform.TransformPoint(zone.GetCenter()), zone.transform.rotation, Vector3.Scale(zone.GetSize(), zone.transform.lossyScale));

            Material.SetPass(0);

            Graphics.DrawMeshNow(Mesh, matrix);

            Handles.color = (gizmoType & GizmoType.Selected) != 0 ? new Color(1f, 0.6f, 0f) : Color.yellow;

            using (new Handles.DrawingScope(matrix))
            {
                Handles.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }
    }
}
