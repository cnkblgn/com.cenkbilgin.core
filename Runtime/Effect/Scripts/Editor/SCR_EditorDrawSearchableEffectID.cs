using UnityEditor;
using Core.Editor;

namespace Core.Effect.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(EffectID))]
    internal sealed class EditorDrawSearchableEffectID : EditorDrawSearchable<string>
    {
        protected override string GetEmpty() => STRING_EMPTY;

        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => EffectDatabase.GetIDs();

        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
