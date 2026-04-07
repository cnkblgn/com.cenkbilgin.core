using UnityEditor;
using UnityEngine;

namespace Core.Localization.Editor
{
    [CustomEditor(typeof(LocalizationDatabaseConfig))]
    public class EditorDrawLocalizationDatabaseConfig : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            GUILayout.Space(10);

            if (GUILayout.Button("Parse"))
            {
                LocalizationDatabaseConfig db = (LocalizationDatabaseConfig)target;

                Undo.RecordObject(target, "Parse Localization Database");

                db.TryParse(); 

                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}