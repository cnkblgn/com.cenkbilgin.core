using UnityEditor;
using Core.Editor;

namespace Core.Actor.Editor
{
    [CustomPropertyDrawer(typeof(ActorTag))]
    internal class EditorDrawActorTag : EditorDrawSearchable
    {
        protected override string[] GetKeys() => ActorDatabase.GetTags();
        protected override string GetKey() => "key";
        protected override void OnApply(SerializedProperty property, string key)
        {
            SerializedProperty prop = property.FindPropertyRelative("index");

            prop.intValue = ActorDatabase.GetTagIndex(key);
        }
    }
}
