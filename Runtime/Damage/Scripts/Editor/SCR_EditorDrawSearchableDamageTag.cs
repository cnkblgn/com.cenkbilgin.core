using UnityEditor;
using Core.Editor;

namespace Core.Damage.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(DamageTag))]
    internal sealed class EditorDrawSearchableDamageTag : EditorDrawSearchable<string>
    {
        protected override void OnApply(SerializedProperty property, string key)
        {
            SerializedProperty indexProperty = property.FindPropertyRelative("index");
            indexProperty.intValue = DamageDatabase.GetIndex(key);
        }

        protected override string GetEmpty() => STRING_EMPTY;

        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => DamageDatabase.GetTags();

        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
