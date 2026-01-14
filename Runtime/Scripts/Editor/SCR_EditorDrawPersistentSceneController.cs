using UnityEngine;
using UnityEditor;

namespace Core.Editor
{
    using static CoreUtility;

    [CustomEditor(typeof(PersistentSceneController))]
    public class EditorDrawPersistentSceneController : UnityEditor.Editor
    {
        private PersistentSceneController baseClass = null;

        private void OnEnable() => baseClass = (PersistentSceneController)target;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            if (GUILayout.Button("Populate"))
            {
                Undo.RecordObject(baseClass, "Populate Persistent Objects");

                baseClass.Populate();

                UnityEditor.EditorUtility.SetDirty(baseClass);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
