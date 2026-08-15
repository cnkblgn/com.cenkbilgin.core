using UnityEditor;
using UnityEngine;
using Core.Editor;

namespace Core.Prefab.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(PrefabID))]
    internal sealed class EditorDrawSearchablePrefabID : EditorDrawSearchable<string>
    {
        private static SearchCollection<string> search;

        private static void Rebuild()
        {
            int prefabs = PrefabDatabase.GetPrefabs();
            search = new SearchCollection<string>(new SearchEntry<string>[prefabs]);

            for (int i = 0; i < prefabs; i++)
            {
                string key = PrefabDatabase.GetID(i).Key;
                search.Entries[i] = new(key, key);
            }
        }
        protected override string GetEmpty() => STRING_EMPTY;
        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys()
        {
            if (search == null)
            {
                Rebuild();
            }

            return search;
        }
        protected override Object GetAsset(string key) => PrefabDatabase.GetPrefab(PrefabDatabase.GetIDIndex(key));
        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
