using UnityEditor;
using Core.Editor;

namespace Core.Localization.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(LocalizedID))]
    public sealed class EditorDrawSearchableLocalizedID : EditorDrawSearchable<string>
    {
        protected override string GetEmpty() => STRING_EMPTY;
        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => LocalizationDatabase.GetKeys();
        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}