using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    using static CoreUtility;

    public abstract class EditorDrawSearchable : PropertyDrawer
    {
        private const float ROW_HEIGHT = 16f;
        private const float SEARCH_HEIGHT = 16f;
        private const float DROPDOWN_HEIGHT = 256f;

        private const string DECOR_UP = "▴";
        private const string DECOR_DOWN = "▾";
        private const string DECOR_NONE = "<None>";

        private const string SEARCH_FOCUS_CONTROL = "NEW_SEARCH_FIELD";

        private class State
        {
            public bool Open = false;
            public bool Dirty = true;
            public bool Focus = false;

            public Vector2 Scroll = Vector2.zero;

            public string Search = STRING_EMPTY;
            public string[] Cached = null;

            public readonly List<int> Filtered = new(64);
        }

        private static readonly Dictionary<EntityId, Dictionary<string, State>> states = new();

        /// <summary> This is used for finding serialized property. Draws named property. Use property name! eg: ItemID </summary>
        protected abstract string GetKey();
        /// <summary> This is used for search list. eg: ItemDatabase.GetKeys(); </summary>
        protected abstract string[] GetKeys();
        /// <summary> This is called before applying key id </summary>
        protected virtual void OnApply(SerializedProperty property, string key) { }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 4f + (GetState(property).Open ? DROPDOWN_HEIGHT : 0f);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty key = property.FindPropertyRelative(GetKey());

            if (key == null)
            {
                Debug.LogWarning($"{GetKey()} named key is not found in property!");
                return;
            }

            State state = GetState(property);

            float lineHeight = EditorGUIUtility.singleLineHeight;

            EditorGUI.BeginProperty(position, label, property);
            {
                EditorGUI.PropertyField(GetFieldRect(position, lineHeight), key, label);

                if (GUI.Button(GetToggleRect(position, lineHeight), state.Open ? DECOR_UP : DECOR_DOWN, EditorStyles.miniButton))
                {
                    state.Open = !state.Open;

                    if (state.Open)
                    {
                        state.Focus = true;
                    }
                }

                if (!state.Open)
                {
                    EditorGUI.EndProperty();
                    return;
                }

                Rect dropdownRect = GetDropdownRect(position, lineHeight, DROPDOWN_HEIGHT);
                EditorGUI.DrawRect(dropdownRect, new Color(0.18f, 0.18f, 0.18f, 0.98f));
                GUI.Box(dropdownRect, GUIContent.none, EditorStyles.helpBox);

                Rect innerRect = GetInnerRect(dropdownRect);

                Rect searchRect = GetSearchRect(innerRect);

                DrawSearch(searchRect, state, out string newSearch);

                if (!string.Equals(newSearch, state.Search, StringComparison.Ordinal))
                {
                    state.Search = newSearch;
                    state.Dirty = true;
                    state.Scroll = Vector2.zero;
                }

                Rect listRect = GetListRect(innerRect);

                DrawList(listRect, property, key, state);
            }
            EditorGUI.EndProperty();
        }

        private void DrawSearch(Rect rect, State state, out string search)
        {
            GUI.SetNextControlName(SEARCH_FOCUS_CONTROL);

            search = EditorGUI.TextField(rect, state.Search, EditorStyles.toolbarSearchField);

            if (state.Focus && Event.current.type == EventType.Repaint)
            {
                state.Focus = false;

                GUI.FocusControl(SEARCH_FOCUS_CONTROL);

                EditorGUI.FocusTextInControl(SEARCH_FOCUS_CONTROL);
            }
        }
        private void DrawList(Rect rect, SerializedProperty property, SerializedProperty keyProperty, State state)
        {
            string[] keys = GetKeys();

            EnsureFilter(state, keys);

            int visibleRowCount = state.Filtered.Count + 1;
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

                for (int i = 0; i < state.Filtered.Count; i++)
                {
                    int index = state.Filtered[i];
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

                OnApply(property, value);
                property.serializedObject.ApplyModifiedProperties();

                state.Open = false;
                GUI.FocusControl(null);
            }
        }

        private Rect GetFieldRect(Rect position, float lineHeight)
        {
            return new
            (
                position.x,
                position.y,
                position.width - 22f,
                lineHeight
            );
        }
        private Rect GetToggleRect(Rect position, float lineHeight)
        {
            return new
            (
                position.xMax - 20f,
                position.y,
                20f,
                lineHeight
            );
        }
        private Rect GetInnerRect(Rect position)
        {
            return new
            (
                position.x + 4f,
                position.y + 4f,
                position.width - 8f,
                position.height - 8f
            );
        }
        private Rect GetSearchRect(Rect position)
        {
            return new
            (
                position.x,
                position.y,
                position.width,
                SEARCH_HEIGHT
            );
        }
        private Rect GetListRect(Rect position)
        {
            return new
            (
                position.x,
                position.y + SEARCH_HEIGHT + 4f,
                position.width,
                position.height - SEARCH_HEIGHT - 4f
            );
        }
        private Rect GetDropdownRect(Rect position, float lineHeight, float height)
        {
            Rect dropdownRect = new
            (
                position.x,
                position.y + lineHeight + 4f,
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
            if (!state.Dirty && ReferenceEquals(state.Cached, keys))
            {
                return;
            }

            state.Filtered.Clear();
            state.Cached = keys;

            bool hasSearch = !string.IsNullOrEmpty(state.Search);

            for (int i = 0; i < keys.Length; i++)
            {
                string id = keys[i];

                if (hasSearch && id.IndexOf(state.Search, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                state.Filtered.Add(i);
            }

            state.Dirty = false;
        }
        private static State GetState(SerializedProperty baseProperty)
        {
            EntityId instanceId = baseProperty.serializedObject.targetObject.GetEntityId();

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
