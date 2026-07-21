using UnityEditor;
using Core.Editor;

namespace Core.Audio.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(SoundID))]
    internal sealed class EditorDrawSearchableSoundID : EditorDrawSearchable<string>
    {
        protected override void OnApply(SerializedProperty property, string key)
        {
            SerializedProperty indexProperty = property.FindPropertyRelative("index");
            indexProperty.intValue = SoundDatabase.GetIndex(new(key, -1));
        }

        protected override string GetEmpty() => STRING_EMPTY;

        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => SoundDatabase.GetIDs();

        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}