using System;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    /// <summary>
    /// Drawer for [SerializeReference, Reference] fields. Works for both a single field
    /// (ItemComponent component) and an array/List field (ItemComponent[] components) —
    /// for arrays/Lists Unity calls this drawer once per element, and fieldInfo still
    /// refers to the containing array/List's FieldInfo, so we resolve the element type.
    /// </summary>
    [CustomPropertyDrawer(typeof(Reference))]
    internal sealed class EditorDrawSearchableReference : EditorDrawSearchable<Type>
    {
        private static readonly GUIStyle GUI_STYLE = new(EditorStyles.objectField);

        // No nested "key" sub-property here — the property itself IS the reference.
        protected override string GetKey() => null;

        protected override bool ShowPingButton => false;

        protected override SearchCollection<Type> GetKeys() => ReferenceDatabase.GetCollection(ReferenceUtility.GetBaseType(fieldInfo));

        protected override Type GetValue(SerializedProperty keyProperty) => keyProperty.managedReferenceValue?.GetType();
        protected override void SetValue(SerializedProperty keyProperty, Type value)
        {
            // GetUninitializedObject never calls any constructor (parameterless or not),
            // matching how Unity itself materializes SerializeReference instances —
            // so types with only parameterized constructors work fine too.
            // Trade-off: inline field initializers won't run, fields start at their
            // C# default (0 / null / false) rather than any "= 10f" you wrote.
            keyProperty.managedReferenceValue = value == null ? null : FormatterServices.GetUninitializedObject(value);
        }

        protected override Type GetEmpty() => null;
        protected override bool ValueEquals(Type a, Type b) => a == b;

        protected override float GetExtraHeight(SerializedProperty property, SerializedProperty keyProperty)
        {
            if (keyProperty.managedReferenceValue == null)
            {
                return 0f;
            }

            float height = 0f;

            SerializedProperty iterator = keyProperty.Copy();
            SerializedProperty end = keyProperty.GetEndProperty();

            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                height += EditorGUI.GetPropertyHeight(iterator, true) + 2f;
                enterChildren = false;
            }

            return height;
        }
        protected override void DrawExtra(Rect rect, SerializedProperty property, SerializedProperty keyProperty)
        {
            if (keyProperty.managedReferenceValue == null)
            {
                return;
            }

            SerializedProperty iterator = keyProperty.Copy();
            SerializedProperty end = keyProperty.GetEndProperty();

            bool enterChildren = true;
            float y = rect.y;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                float height = EditorGUI.GetPropertyHeight(iterator, true);

                Rect fieldRect = new(rect.x, y, rect.width, height);

                EditorGUI.PropertyField(fieldRect, iterator, true);

                y += height + 2f;
                enterChildren = false;
            }
        }
        protected override void DrawField(Rect rect, SerializedProperty property, SerializedProperty keyProperty, GUIContent label)
        {
            Type type = GetValue(keyProperty);

            Rect left = rect;
            left.width = EditorGUIUtility.labelWidth;

            Rect right = rect;
            right.x = left.xMax;
            right.width = rect.xMax - left.xMax;

            EditorGUI.LabelField(left, label.text);
            EditorGUI.LabelField(right, type == null ? "<None>" : type.Name, GUI_STYLE);

            Rect foldoutRect = rect;
            foldoutRect.width = 0;

            keyProperty.isExpanded = EditorGUI.Foldout(foldoutRect, keyProperty.isExpanded, GUIContent.none, false);
        }

        protected override bool Validate(out string error) => ReferenceUtility.Validate(fieldInfo, out error);
    }
}
