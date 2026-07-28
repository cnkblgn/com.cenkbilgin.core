using UnityEditor;
using Core.Editor;

namespace Core.Stat.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(StatID))]
    internal sealed class EditorDrawSearchableStatID : EditorDrawSearchable<string>
    {
        protected override string GetEmpty() => STRING_EMPTY;

        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => StatDatabase.GetIDs();

        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
