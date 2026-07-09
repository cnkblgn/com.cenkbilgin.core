using UnityEditor;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(PrefabID))]
    internal class EditorDrawPrefabID : EditorDrawSearchable
    {
        protected override string[] GetKeys() => PrefabDatabase.GetIDs();
        protected override string GetKey() => "key";
    }
}
