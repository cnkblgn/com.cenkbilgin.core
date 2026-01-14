using UnityEditor;
using UnityEngine;
using Core;

namespace Core.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(ReadOnly))]
    public class EditorPropertyReadonly : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }
}