using UnityEditor;
using Core.Editor;

namespace Core.Actors.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(ActorID))]
    internal sealed class EditorDrawSearchableActorID : EditorDrawSearchable<string>
    {
        protected override void OnApply(SerializedProperty property, string key)
        {
            SerializedProperty indexProperty = property.FindPropertyRelative("index");
            indexProperty.intValue = ActorDatabase.GetIDIndex(key);
        }

        protected override string GetEmpty() => STRING_EMPTY;

        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => ActorDatabase.GetIDs();

        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
