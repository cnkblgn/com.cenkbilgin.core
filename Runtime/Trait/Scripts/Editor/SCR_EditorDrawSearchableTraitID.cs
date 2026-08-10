using UnityEditor;
using Core.Editor;

namespace Core.Trait.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(TraitID))]
    internal sealed class EditorDrawSearchableTraitID : EditorDrawSearchable<string>
    {
        protected override void OnApply(SerializedProperty property, string key, int index)
        {
            SerializedProperty indexProperty = property.FindPropertyRelative("index");
            indexProperty.intValue = TraitDatabase.GetIndex(key);
        }

        protected override string GetEmpty() => STRING_EMPTY;

        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => TraitDatabase.GetIDs();

        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
