using UnityEditor;
using Core.Editor;

namespace Core.Item.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(ItemID))]
    internal sealed class EditorDrawSearchableItemID : EditorDrawSearchable<string>
    {
        protected override string GetEmpty() => STRING_EMPTY;

        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => ItemDatabase.GetIDs();

        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
