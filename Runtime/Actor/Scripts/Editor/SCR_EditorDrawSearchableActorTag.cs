using UnityEditor;
using Core.Editor;

namespace Core.Actors.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(ActorTag))]
    internal sealed class EditorDrawSearchableActorTag : EditorDrawSearchable<string>
    {
        protected override string GetEmpty() => STRING_EMPTY;

        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => ActorDatabase.GetTags();

        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
