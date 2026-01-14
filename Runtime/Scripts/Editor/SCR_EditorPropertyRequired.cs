using UnityEditor;
using UnityEngine;
using Core;

namespace Core.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(Required))]
    public class EditorPropertyRequired : PropertyDrawer
    {
        private static Texture2D iconTexture = null;
        private readonly string iconName = "TEX_IconWarning";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();

            bool isEmpty = IsFieldEmpty(property);
            Rect fieldRect = new(position.x, position.y, position.width - 20, position.height);

            if (isEmpty)
            {
                EditorGUI.DrawRect(fieldRect, COLOR_RED);
            }

            EditorGUI.PropertyField(fieldRect, property, label);
            
            if (isEmpty)
            {
                EditorUtility.DrawOutline(fieldRect, COLOR_YELLOW, 1);
                
                Rect iconRect = new(position.xMax - 18, fieldRect.y, 16, 16);

                if (iconTexture == null)
                {
                    string[] guids = AssetDatabase.FindAssets($"t:Texture2D {iconName}");

                    if (guids.Length <= 0)
                    {
                        LogError($"EditorPropertyRequired.OnGUI() Could not find texture: {iconName}");
                        return;
                    }

                    iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }

                GUI.Label(iconRect, new GUIContent(iconTexture, "This field is required and is either missing or empty!"));
            }

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();

                UnityEditor.EditorUtility.SetDirty(property.serializedObject.targetObject);

                EditorApplication.RepaintHierarchyWindow();
            }

            EditorGUI.EndProperty(); 
        }
        private bool IsFieldEmpty(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                // Add additional types as necessary
                case SerializedPropertyType.ObjectReference when property.objectReferenceValue:
                case SerializedPropertyType.ExposedReference when property.exposedReferenceValue:
                case SerializedPropertyType.AnimationCurve when property.animationCurveValue is { length: > 0 }:
                case SerializedPropertyType.String when !string.IsNullOrEmpty(property.stringValue):
                    return false;
                default:
                    return true;
            }
        }
    }
}