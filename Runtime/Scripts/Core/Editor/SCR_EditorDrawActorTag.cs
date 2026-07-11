using System.Collections.Generic;
using UnityEditor;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(ActorTag))]
    internal class EditorDrawActorTag : EditorDrawSearchable
    {
        protected override IReadOnlyList<string> GetKeys() => ActorDatabase.GetTags();
        protected override string GetKey() => "key";
        protected override void OnApply(SerializedProperty property, string key)
        {
            SerializedProperty prop = property.FindPropertyRelative("index");

            prop.intValue = ActorDatabase.GetTagIndex(key);
        }
    }
}
