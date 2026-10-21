using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(Expandable))]
    public sealed class EditorDrawExpandable : PropertyDrawer
    {
        private const float INDENT_WIDTH = 15f;

        private sealed class CacheEntry
        {
            public Object Target;
            public SerializedObject SerializedObject;
        }

        private static readonly Dictionary<int, CacheEntry> CACHE = new();

        static EditorDrawExpandable()
        {
            AssemblyReloadEvents.beforeAssemblyReload += ClearCache;
            EditorApplication.quitting += ClearCache;
        }

        private static void ClearCache()
        {
            CACHE.Clear();
        }

        private static SerializedObject GetSerializedObject(Object target)
        {
            int id = target.GetInstanceID();

            if (CACHE.TryGetValue(id, out CacheEntry entry))
            {
                if (entry.Target == target &&
                    entry.SerializedObject != null)
                {
                    return entry.SerializedObject;
                }
            }

            SerializedObject serializedObject = new(target);

            CACHE[id] = new CacheEntry
            {
                Target = target,
                SerializedObject = serializedObject
            };

            return serializedObject;
        }

        private static bool IsValid(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ObjectReference
                && (property.objectReferenceValue == null
                    || property.objectReferenceValue is ScriptableObject);
        }

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (!IsValid(property))
            {
                EditorGUI.HelpBox(
                    position,
                    "[Expandable] requires a ScriptableObject reference.",
                    MessageType.Error
                );

                EditorGUI.EndProperty();
                return;
            }

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect fieldRect = new(
                position.x,
                position.y,
                position.width,
                lineHeight
            );

            EditorGUI.PropertyField(fieldRect, property, label);

            Object target = property.objectReferenceValue;

            if (target != null)
            {
                property.isExpanded = EditorGUI.Foldout(
                    fieldRect,
                    property.isExpanded,
                    GUIContent.none,
                    true
                );

                if (property.isExpanded)
                {
                    SerializedObject serializedObject =
                        GetSerializedObject(target);

                    serializedObject.UpdateIfRequiredOrScript();

                    Rect contentRect = new(
                        position.x + INDENT_WIDTH,
                        position.y + lineHeight + spacing,
                        position.width - INDENT_WIDTH,
                        position.height - lineHeight - spacing
                    );

                    EditorGUI.indentLevel++;

                    SerializedProperty iterator =
                        serializedObject.GetIterator();

                    bool enterChildren = true;
                    float y = contentRect.y;

                    while (iterator.NextVisible(enterChildren))
                    {
                        enterChildren = false;

                        if (iterator.propertyPath == "m_Script")
                            continue;

                        float height =
                            EditorGUI.GetPropertyHeight(iterator, true);

                        EditorGUI.PropertyField(
                            new Rect(
                                contentRect.x,
                                y,
                                contentRect.width,
                                height
                            ),
                            iterator,
                            true
                        );

                        y += height + spacing;
                    }

                    EditorGUI.indentLevel--;

                    serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (!IsValid(property))
                return height * 2f;

            if (property.objectReferenceValue == null ||
                !property.isExpanded)
            {
                return height;
            }

            float spacing = EditorGUIUtility.standardVerticalSpacing;

            SerializedObject serializedObject =
                GetSerializedObject(property.objectReferenceValue);

            serializedObject.UpdateIfRequiredOrScript();

            SerializedProperty iterator = serializedObject.GetIterator();

            if (!iterator.NextVisible(true))
                return height;

            do
            {
                if (iterator.propertyPath == "m_Script")
                    continue;

                height +=
                    EditorGUI.GetPropertyHeight(iterator, true) + spacing;
            }
            while (iterator.NextVisible(false));

            return height + spacing;
        }
    }
}