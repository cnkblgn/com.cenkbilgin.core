using UnityEditor;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(PrefabID))]
    public class EditorDrawPrefabID : EditorDrawKey
    {
        protected override string[] GetIDs() => EditorUtilityPrefab.GetDatabase().GetKeys();
        protected override string GetKey() => "key";
    }
}
