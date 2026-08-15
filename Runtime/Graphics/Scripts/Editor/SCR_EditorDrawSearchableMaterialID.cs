using UnityEditor;
using UnityEngine;
using Core.Editor;

namespace Core.Graphics.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(MaterialID))]
    public sealed class EditorDrawSearchableMaterialID : EditorDrawSearchable<string>
    {
        private static SearchCollection<string> search;

        private static void Rebuild()
        {
            int materials = MaterialDatabase.GetMaterials();
            search = new SearchCollection<string>(new SearchEntry<string>[materials]);

            for (int i = 0; i < materials; i++)
            {
                string key = MaterialDatabase.GetID(i).Key;
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
        protected override Object GetAsset(string key) => MaterialDatabase.GetMaterial(MaterialDatabase.GetIDIndex(key));
        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
