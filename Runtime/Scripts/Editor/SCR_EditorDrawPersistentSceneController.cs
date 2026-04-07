using UnityEngine;
using UnityEditor;

namespace Core.Editor
{
    [CustomEditor(typeof(PersistentSceneController))]
    public class EditorDrawPersistentSceneController : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            GUILayout.Space(10);

            if (GUILayout.Button("Populate"))
            {
                PersistentSceneController controller = (PersistentSceneController)target;

                Undo.RecordObject(controller, "Populate Persistent Objects");

                controller.Populate();

                UnityEditor.EditorUtility.SetDirty(controller);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
