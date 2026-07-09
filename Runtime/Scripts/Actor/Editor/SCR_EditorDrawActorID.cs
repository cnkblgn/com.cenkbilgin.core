using UnityEditor;
using Core.Editor;

namespace Core.Actor.Editor
{
    [CustomPropertyDrawer(typeof(ActorID))]
    internal class EditorDrawActorID : EditorDrawSearchable
    {
        protected override string[] GetKeys() => ActorDatabase.GetIDs();
        protected override string GetKey() => "key";
    }
}
