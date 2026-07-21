using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(ReadOnly))]
    internal sealed class EditorDrawReadonly : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }
}