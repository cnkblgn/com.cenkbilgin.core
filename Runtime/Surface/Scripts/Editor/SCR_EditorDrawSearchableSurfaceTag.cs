using UnityEditor;
using Core.Editor;

namespace Core.Surface.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(SurfaceTag))]
    internal sealed class EditorDrawSearchableSurfaceTag : EditorDrawSearchable<string>
    {
        protected override void OnApply(SerializedProperty property, string key)
        {
            SerializedProperty indexProperty = property.FindPropertyRelative("index");
            indexProperty.intValue = SurfaceDatabase.GetIndex(key);
        }

        protected override string GetEmpty() => STRING_EMPTY;

        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => SurfaceDatabase.GetIDs();

        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
