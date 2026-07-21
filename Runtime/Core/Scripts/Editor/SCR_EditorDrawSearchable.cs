using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    using static CoreUtility;

    public abstract class EditorDrawSearchable<T> : PropertyDrawer
    {
        private const float PING_BUTTON_SIZE = 20f;
        private const string PING_BUTTON_ICON = "◉";

        private const float ROW_HEIGHT = 16f;
        private const float SEARCH_HEIGHT = 16f;
        private const float DROPDOWN_HEIGHT = 256f;

        private const string SEARCH_UP_ICON = "▴";
        private const string SEARCH_DOWN_ICON = "▾";
        private const string SEARCH_NONE_TEXT = "<None>";

        private const string SEARCH_FOCUS_CONTROL = "NEW_SEARCH_FIELD";

        private static readonly Color SELECTED_COLOR = new(0.25f, 0.45f, 0.8f, 0.35f);
        private static readonly Color DROPDOWN_COLOR = new(0.18f, 0.18f, 0.18f, 0.98f);

        // Static dictionary is per closed generic type (per T), so different
        // drawers (string, enum, Type, ...) never collide on the same keyProperty.
        private static readonly Dictionary<SearchKey, SearchState<T>> states = new();

        /// <summary> Set to false to hide the ping button entirely (eg. when there's no asset concept, like SerializeReference types). </summary>
        protected virtual bool ShowPingButton => true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!Validate(out string error))
            {
                EditorGUI.HelpBox(position, error, MessageType.Error);
                return;
            }

            SerializedProperty keyProperty = ResolveKeyProperty(property);

            if (keyProperty == null)
            {
                Debug.LogWarning($"{GetKey()} named key is not found in property!");
                return;
            }

            SearchState<T> state = GetState(property);

            float lineHeight = EditorGUIUtility.singleLineHeight;

            EditorGUI.BeginProperty(position, label, property);
            {
                DrawField(GetFieldRect(position, lineHeight), property, keyProperty, label);

                if (ShowPingButton && GUI.Button(GetPingRect(position, lineHeight), PING_BUTTON_ICON, EditorStyles.miniButton))
                {
                    UnityEngine.Object asset = GetAsset(GetValue(keyProperty));

                    if (asset != null)
                    {
                        EditorGUIUtility.PingObject(asset);
                        Selection.activeObject = asset;
                    }
                }

                if (GUI.Button(GetToggleRect(position, lineHeight), state.Open ? SEARCH_UP_ICON : SEARCH_DOWN_ICON, EditorStyles.miniButton))
                {
                    state.Open = !state.Open;

                    if (state.Open)
                    {
                        state.Focus = true;
                    }
                }

                if (state.Open)
                {
                    Rect dropdownRect = GetDropdownRect(position, lineHeight, DROPDOWN_HEIGHT);

                    EditorGUI.DrawRect(dropdownRect, DROPDOWN_COLOR);
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

                    DrawList(listRect, property, keyProperty, state);
                }
                else if (keyProperty.isExpanded)
                {
                    float extraHeight = GetExtraHeight(property, keyProperty);

                    if (extraHeight > 0f)
                    {
                        Rect extraRect = new
                        (
                            position.x,
                            position.y + lineHeight + 4f,
                            position.width,
                            extraHeight
                        );

                        DrawExtra(extraRect, property, keyProperty);
                    }
                }
            }
            EditorGUI.EndProperty();
        }
        /// <summary> This is called before applying value </summary>
        protected virtual void OnApply(SerializedProperty property, T value) { }

        /// <summary> Draws the extra content reserved by GetExtraHeight. </summary>
        protected virtual void DrawExtra(Rect rect, SerializedProperty property, SerializedProperty keyProperty) { }
        /// <summary> Draws the header row's field control. Override to replace the default PropertyField (eg. with a custom Foldout). </summary>
        protected virtual void DrawField(Rect rect, SerializedProperty property, SerializedProperty keyProperty, GUIContent label) => EditorGUI.PropertyField(rect, keyProperty, label);
        private void DrawSearch(Rect rect, SearchState<T> state, out string search)
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
        private void DrawList(Rect rect, SerializedProperty property, SerializedProperty keyProperty, SearchState<T> state)
        {
            SearchCollection<T> collection = GetKeys();

            EnsureFilter(state, collection);

            int visibleRowCount = state.Filtered.Count + 1;
            float contentHeight = visibleRowCount * ROW_HEIGHT;

            Rect viewRect = new
            (
                0f,
                0f,
                rect.width,
                contentHeight
            );

            T current = GetValue(keyProperty);

            state.Scroll = GUI.BeginScrollView(rect, state.Scroll, viewRect);
            {
                float y = 0f;

                DrawRow(ref y, viewRect.width, SEARCH_NONE_TEXT, property, keyProperty, state, current, GetEmpty());

                for (int i = 0; i < state.Filtered.Count; i++)
                {
                    SearchEntry<T> entry = state.Filtered[i];

                    DrawRow(ref y, viewRect.width, entry.Label, property, keyProperty, state, current, entry.Value);
                }
            }
            GUI.EndScrollView();
        }
        private void DrawRow(ref float height, float width, string label, SerializedProperty property, SerializedProperty keyProperty, SearchState<T> state, T current, T value)
        {
            Rect rowRect = new
            (
                0f,
                height,
                width,
                ROW_HEIGHT
            );

            height += ROW_HEIGHT;

            bool selected = ValueEquals(current, value);

            if (selected)
            {
                EditorGUI.DrawRect(rowRect, SELECTED_COLOR);
            }

            if (GUI.Button(rowRect, label, EditorStyles.miniButton))
            {
                SetValue(keyProperty, value);

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
                position.width - PING_BUTTON_SIZE,
                lineHeight
            );
        }
        private Rect GetPingRect(Rect position, float lineHeight)
        {
            return new
            (
                position.xMax - PING_BUTTON_SIZE * 2,
                position.y,
                PING_BUTTON_SIZE,
                lineHeight
            );
        }
        private Rect GetToggleRect(Rect position, float lineHeight)
        {
            return new
            (
                position.xMax - PING_BUTTON_SIZE,
                position.y,
                PING_BUTTON_SIZE,
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
            return new
            (
                position.x,
                position.y + lineHeight + 4f,
                position.width,
                height
            );
        }

        private static SearchState<T> GetState(SerializedProperty property)
        {
            SearchKey key = new(property.serializedObject.targetObject.GetEntityId(), property.propertyPath);

            if (!states.TryGetValue(key, out SearchState<T> state))
            {
                state = new SearchState<T>();
                states.Add(key, state);
            }

            return state;
        }
        /// <summary> Extra height drawn below the header row when the dropdown is closed (eg. nested fields of a SerializeReference instance). </summary>
        protected virtual float GetExtraHeight(SerializedProperty property, SerializedProperty keyProperty) => 0f;
        protected virtual float GetValidationHeight() => EditorGUIUtility.singleLineHeight * 2.5f;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!Validate(out _))
            {
                return GetValidationHeight();
            }

            float height = EditorGUIUtility.singleLineHeight + 4f;

            SearchState<T> state = GetState(property);

            if (state.Open)
            {
                return height + DROPDOWN_HEIGHT;
            }

            SerializedProperty key = ResolveKeyProperty(property);

            if (key != null && key.isExpanded)
            {
                height += GetExtraHeight(property, key);
            }

            return height;
        }
        /// <summary> This is used for finding serialized property. Draws named property. Use property name! eg: ItemID. Return null/empty to operate on the property itself (eg. SerializeReference fields). </summary>
        protected abstract string GetKey();
        /// <summary> This is used for search list. eg: ItemDatabase.GetSearchCollection(); </summary>
        protected abstract SearchCollection<T> GetKeys();
        /// <summary> This is used for pinging asset </summary>
        protected virtual UnityEngine.Object GetAsset(T value) => null;
        /// <summary> Reads the currently stored value out of the keyProperty property. </summary>
        protected abstract T GetValue(SerializedProperty keyProperty);
        /// <summary> Writes the selected value into the keyProperty property. </summary>
        protected abstract void SetValue(SerializedProperty keyProperty, T value);
        /// <summary> Value that represents the "&lt;None&gt;" row. Override if default(T) isn't correct (eg. string.Empty). </summary>
        protected virtual T GetEmpty() => default;

        /// <summary> Validates field </summary>
        protected virtual bool Validate(out string error)
        {
            error = null;
            return true;
        }
        /// <summary> Compares two values to figure out which row is selected. Override for custom equality. </summary>
        protected virtual bool ValueEquals(T a, T b) => EqualityComparer<T>.Default.Equals(a, b);
        private void EnsureFilter(SearchState<T> state, SearchCollection<T> collection)
        {
            if (!state.Dirty && ReferenceEquals(state.Cached, collection))
            {
                return;
            }

            state.Filtered.Clear();
            state.Cached = collection;

            bool hasSearch = !string.IsNullOrEmpty(state.Search);

            IReadOnlyList<SearchEntry<T>> entries = collection.Entries;

            for (int i = 0; i < entries.Count; i++)
            {
                SearchEntry<T> entry = entries[i];

                if (hasSearch && entry.Label.IndexOf(state.Search, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                state.Filtered.Add(entry);
            }

            state.Dirty = false;
        }
        private SerializedProperty ResolveKeyProperty(SerializedProperty property)
        {
            string keyName = GetKey();

            return string.IsNullOrEmpty(keyName) ? property : property.FindPropertyRelative(keyName);
        }
    }
}
