using UnityEditor;
using Core.Editor;

namespace Core.Prefab.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(PrefabID))]
    internal sealed class EditorDrawSearchablePrefabID : EditorDrawSearchable<string>
    {
        protected override string GetEmpty() => STRING_EMPTY;
        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => PrefabDatabase.GetIDs();
        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
