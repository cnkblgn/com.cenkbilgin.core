using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Core.Localization.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(LocalizedString))]
    public class EditorDrawLocalizedString : PropertyDrawer
    {
        private class State
        {
            public bool Open;
            public string Search = "";
            public Vector2 Scroll;

            public bool FilterDirty = true;
            public string[] CachedKeys;
            public readonly List<int> FilteredIndices = new(64);
        }

        private static readonly Dictionary<int, Dictionary<string, State>> states = new();

        private const float ROW_HEIGHT = 16f;
        private const float SEARCH_HEIGHT = 16f;
        private const float DROPDOWN_HEIGHT = 256f;
        private const string KEY = "key";
        private const string DECOR_UP = "▴";
        private const string DECOR_DOWN = "▾";
        private const string DECOR_NONE = "<None>";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 4f + (GetState(property).Open ? DROPDOWN_HEIGHT : 0f);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty keyProp = property.FindPropertyRelative(KEY);
            State state = GetState(property);

            float lineHeight = EditorGUIUtility.singleLineHeight;

            EditorGUI.BeginProperty(position, label, property);
            {
                Rect fieldRect = new
                (
                    position.x, 
                    position.y, 
                    position.width - 22f, 
                    lineHeight
                );

                Rect toggleRect = new
                (
                    position.xMax - 20f,
                    position.y, 
                    20f, 
                    lineHeight
                );

                EditorGUI.PropertyField(fieldRect, keyProp, label);

                if (GUI.Button(toggleRect, state.Open ? DECOR_UP : DECOR_DOWN, EditorStyles.miniButton))
                {
                    state.Open = !state.Open;
                }

                if (!state.Open)
                {
                    EditorGUI.EndProperty();
                    return;
                }

                Rect dropdownRect = GetDropdownRect(position, lineHeight, DROPDOWN_HEIGHT);

                EditorGUI.DrawRect(dropdownRect, new Color(0.18f, 0.18f, 0.18f, 0.98f));
                GUI.Box(dropdownRect, GUIContent.none, EditorStyles.helpBox);

                Rect innerRect = new
                (
                    dropdownRect.x + 4f,
                    dropdownRect.y + 4f,
                    dropdownRect.width - 8f,
                    dropdownRect.height - 8f
                );

                Rect searchRect = new
                (
                    innerRect.x,
                    innerRect.y,
                    innerRect.width,
                    SEARCH_HEIGHT
                );

                string newSearch = EditorGUI.TextField(searchRect, state.Search, EditorStyles.toolbarSearchField);

                if (!string.Equals(newSearch, state.Search, StringComparison.Ordinal))
                {
                    state.Search = newSearch;
                    state.FilterDirty = true;
                    state.Scroll = Vector2.zero;
                }

                Rect listRect = new
                (
                    innerRect.x,
                    innerRect.y + SEARCH_HEIGHT + 4f,
                    innerRect.width,
                    innerRect.height - SEARCH_HEIGHT - 4f
                );

                DrawList(listRect, property, keyProp, state);
            }
            EditorGUI.EndProperty();
        }

        private void DrawList(Rect rect, SerializedProperty property, SerializedProperty keyProperty, State state)
        {
            LocalizationDatabaseConfig db = LocalizationUtility.GetDatabase();

            string[] keys = db.Keys;

            EnsureFilter(state, keys);

            int visibleRowCount = state.FilteredIndices.Count + 1;
            float contentHeight = visibleRowCount * ROW_HEIGHT;

            Rect viewRect = new
            (
                0f, 
                0f, 
                rect.width, 
                contentHeight
            );

            state.Scroll = GUI.BeginScrollView(rect, state.Scroll, viewRect);
            {
                float y = 0f;

                DrawRow(ref y, viewRect.width, DECOR_NONE, property, keyProperty, state, STRING_EMPTY);

                for (int i = 0; i < state.FilteredIndices.Count; i++)
                {
                    int index = state.FilteredIndices[i];
                    string id = keys[index];

                    DrawRow(ref y, viewRect.width, id, property, keyProperty, state, id);
                }
            }
            GUI.EndScrollView();
        }
        private void DrawRow(ref float height, float width, string label, SerializedProperty property, SerializedProperty keyProperty, State state, string value)
        {
            Rect rowRect = new
            (
                0f, 
                height, 
                width, 
                ROW_HEIGHT
            );

            height += ROW_HEIGHT;

            bool selected = keyProperty.stringValue == value;

            if (selected)
            {
                EditorGUI.DrawRect(rowRect, new Color(0.25f, 0.45f, 0.8f, 0.35f));
            }

            if (GUI.Button(rowRect, label, EditorStyles.miniButton))
            {
                keyProperty.stringValue = value;
                property.serializedObject.ApplyModifiedProperties();

                state.Open = false;
                GUI.FocusControl(null);
            }
        }
        private Rect GetDropdownRect(Rect position, float lineH, float height)
        {
            Rect dropdownRect = new
            (
                position.x,
                position.y + lineH + 4f,
                position.width,
                height
            );

            float bottom = position.y + position.height;

            if (dropdownRect.yMax > bottom)
            {
                float overflow = dropdownRect.yMax - bottom;
                dropdownRect.height = Mathf.Max(80f, dropdownRect.height - overflow);
            }

            return dropdownRect;
        }

        private void EnsureFilter(State state, string[] keys)
        {
            if (!state.FilterDirty && ReferenceEquals(state.CachedKeys, keys))
            {
                return;
            }

            state.FilteredIndices.Clear();
            state.CachedKeys = keys;

            bool hasSearch = !string.IsNullOrEmpty(state.Search);

            for (int i = 0; i < keys.Length; i++)
            {
                string id = keys[i];

                if (hasSearch && id.IndexOf(state.Search, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                state.FilteredIndices.Add(i);
            }

            state.FilterDirty = false;
        }
        private static State GetState(SerializedProperty baseProperty)
        {
            int instanceId = baseProperty.serializedObject.targetObject.GetInstanceID();

            string path = baseProperty.propertyPath;

            if (!states.TryGetValue(instanceId, out Dictionary<string, State> objectStates))
            {
                objectStates = new Dictionary<string, State>();
                states.Add(instanceId, objectStates);
            }

            if (!objectStates.TryGetValue(path, out State state))
            {
                state = new State();
                objectStates.Add(path, state);
            }

            return state;
        }
    }
}