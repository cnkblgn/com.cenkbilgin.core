using UnityEditor;
using UnityEngine;

namespace Core.Actors.Editor
{
    using static CoreUtility;

    [InitializeOnLoad]
    internal sealed class EditorGizmoActor
    {
        private static GUIStyle GizmosStyle
        {
            get
            {
                gizmosStyle ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, };
                gizmosStyle.normal.textColor = COLOR_GREEN;

                return gizmosStyle;
            }
        }
        private static GUIStyle gizmosStyle;

        static EditorGizmoActor() { }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void Draw(Actor actor, GizmoType gizmoType)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            bool selected = (gizmoType & GizmoType.Selected) != 0;
            Gizmos.color = selected ? Color.red : Color.greenYellow;
            Gizmos.DrawWireSphere(actor.Origin.position, 0.1f);

            using (new Handles.DrawingScope())
            {
                Handles.Label(actor.Origin.position + Vector3.up, actor.ID.Key, GizmosStyle);
            }
        }
    }
}
