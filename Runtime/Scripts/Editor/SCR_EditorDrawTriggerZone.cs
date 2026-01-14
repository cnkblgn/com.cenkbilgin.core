using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    using static CoreUtility;

    [CustomEditor(typeof(TriggerZone))]
    public class EditorDrawTriggerZone : UnityEditor.Editor
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawTriggerZoneGizmo(TriggerZone triggerZone, GizmoType _)
        {
            if (triggerZone == null)
            {
                return;
            }

            if (!triggerZone.TryGetComponent(out Collider triggerCollider))
            {
                return;
            }

            Transform triggerTransform = triggerCollider.transform;

            // Draw collider shape
            if (triggerCollider is BoxCollider box)
            {
                Gizmos.matrix = Matrix4x4.TRS(triggerTransform.position, triggerTransform.rotation, triggerTransform.lossyScale);

                Gizmos.color = COLOR_GREEN;
                Gizmos.DrawWireCube(box.center, box.size * 1.05f);

                Gizmos.color = COLOR_BLUE.Alpha(0.25f);
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (triggerCollider is SphereCollider sphere)
            {
                Gizmos.matrix = Matrix4x4.TRS(triggerTransform.position, triggerTransform.rotation, triggerTransform.lossyScale);

                Gizmos.color = COLOR_GREEN;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius * 1.05f);

                Gizmos.color = COLOR_BLUE.Alpha(0.25f);
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
            else if (triggerCollider is CapsuleCollider capsule)
            {
                Matrix4x4 matrix = Matrix4x4.TRS(triggerTransform.position, triggerTransform.rotation, triggerTransform.lossyScale);
                Gizmos.color = COLOR_BLUE.Alpha(0.25f);
                Gizmos.matrix = matrix;

                Vector3 center = capsule.center;
                float radius = capsule.radius;
                float height = Mathf.Max(capsule.height, radius * 2);

                Vector3 axis, up, forward;
                switch (capsule.direction)
                {
                    case 0: axis = Vector3.right; up = Vector3.up; forward = Vector3.forward; break;
                    case 1: axis = Vector3.up; up = Vector3.forward; forward = Vector3.right; break;
                    default: axis = Vector3.forward; up = Vector3.up; forward = Vector3.right; break;
                }

                float halfHeight = (height / 2) - radius;
                Vector3 top = center + axis * halfHeight;
                Vector3 bottom = center - axis * halfHeight;

                Gizmos.DrawSphere(top, radius);
                Gizmos.DrawSphere(bottom, radius);

                Gizmos.color = COLOR_GREEN;
                for (int i = 0; i < 8; i++)
                {
                    float angle0 = (i / (float)8) * Mathf.PI * 2;
                    float angle1 = ((i + 1) / (float)8) * Mathf.PI * 2;

                    Vector3 offset0 = Mathf.Cos(angle0) * radius * up + forward * Mathf.Sin(angle0) * radius;
                    Vector3 offset1 = Mathf.Cos(angle1) * radius * up + forward * Mathf.Sin(angle1) * radius;

                    Gizmos.DrawLine(top + offset0, top + offset1);
                    Gizmos.DrawLine(bottom + offset0, bottom + offset1);
                    Gizmos.DrawLine(top + offset0, bottom + offset0);
                }
            }

            Handles.matrix = Matrix4x4.identity;
            Handles.color = COLOR_WHITE;

            GUIStyle style = new();
            style.normal.textColor = COLOR_RED;
            style.alignment = TextAnchor.MiddleCenter;
            Handles.Label(triggerCollider.bounds.center + Vector3.up, triggerZone.name.ToBold(), style);
        }
    }
}