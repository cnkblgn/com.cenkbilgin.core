using UnityEditor;
using UnityEngine;
using Core.Editor;

namespace Core.Graphics.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(IconID))]
    public sealed class EditorDrawSearchableIconID : EditorDrawSearchable<string>
    {
        protected override void OnApply(SerializedProperty property, string key, int index)
        {
            SerializedProperty indexProperty = property.FindPropertyRelative("index");
            indexProperty.intValue = IconDatabase.GetIndex(key);
        }


        protected override string GetEmpty() => STRING_EMPTY;

        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => IconDatabase.GetIDs();

        protected override Object GetAsset(string key) => IconDatabase.GetSprite(key);

        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
