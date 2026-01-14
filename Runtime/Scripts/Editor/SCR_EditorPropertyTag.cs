using UnityEngine;
using UnityEditor;

namespace Core.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(Tag))]
    public class EditorPropertyTag : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, LOG_ERROR + "Incompatible type!");
            }
        }
    }
}
