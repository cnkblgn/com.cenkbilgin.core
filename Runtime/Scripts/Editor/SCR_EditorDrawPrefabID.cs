using UnityEditor;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(PrefabID))]
    public class EditorDrawPrefabID : EditorDrawKey
    {
        protected override string[] GetIDs() => PrefabDatabase.GetKeys();
        protected override string GetKey() => "key";
    }
}
