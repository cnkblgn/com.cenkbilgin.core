using UnityEngine;
using UnityEditor;
using Core;

namespace Core.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(Int3))]
    public class EditorPropertyInt3 : PropertyDrawer
    {
        private SerializedProperty propertyX = null;
        private SerializedProperty propertyY = null;
        private SerializedProperty propertyZ = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (propertyX == null)
            {
                propertyX = property.FindPropertyRelative("x");
            }

            if (propertyY == null)
            {
                propertyY = property.FindPropertyRelative("y");
            }

            if (propertyZ == null)
            {
                propertyZ = property.FindPropertyRelative("z");
            }

            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Keyboard), label);

            float labelWidth = 15;
            float totalWidth = position.width;
            float fieldWidth = (totalWidth - labelWidth * 3) / 3;

            Rect xLabelRect = new(position.x, position.y, labelWidth, position.height);
            Rect xFieldRect = new(position.x + labelWidth, position.y, fieldWidth, position.height);
            Rect yLabelRect = new(xFieldRect.x + fieldWidth + 3, position.y, labelWidth, position.height);
            Rect yFieldRect = new(yLabelRect.x + labelWidth, position.y, fieldWidth - 3, position.height);
            Rect zLabelRect = new(yFieldRect.x + fieldWidth - 1, position.y, labelWidth, position.height);
            Rect zFieldRect = new(zLabelRect.x + labelWidth, position.y, fieldWidth - 3, position.height);

            EditorGUI.LabelField(xLabelRect, "X");
            EditorGUI.PropertyField(xFieldRect, propertyX, GUIContent.none);

            EditorGUI.LabelField(yLabelRect, "Y");
            EditorGUI.PropertyField(yFieldRect, propertyY, GUIContent.none);

            EditorGUI.LabelField(zLabelRect, "Z");
            EditorGUI.PropertyField(zFieldRect, propertyZ, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}