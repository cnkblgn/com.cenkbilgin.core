using UnityEditor;
using Core.Editor;
using UnityEngine;

namespace Core.Item.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(ItemTag))]
    internal sealed class EditorDrawSearchableItemTag : EditorDrawSearchable<string>
    {
        protected override void OnApply(SerializedProperty property, string key, int index)
        {
            SerializedProperty indexProperty = property.FindPropertyRelative("index");
            indexProperty.intValue = ItemDatabase.GetTagIndex(key);
        }

        protected override string GetEmpty() => STRING_EMPTY;

        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => ItemDatabase.GetTags();

        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
