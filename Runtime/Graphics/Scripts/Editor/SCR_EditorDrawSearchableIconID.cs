using UnityEngine;
using UnityEditor;
using Core.Editor;

namespace Core.Graphics.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(IconID))]
    public sealed class EditorDrawSearchableIconID : EditorDrawSearchable<string>
    {
        private static SearchCollection<string> search;
        private static int searchVersion = -1;

        private static void Rebuild()
        {
            int sprites = IconDatabase.GetSprites();
            search = new SearchCollection<string>(new SearchEntry<string>[sprites]);

            for (int i = 0; i < sprites; i++)
            {
                string key = IconDatabase.GetID(i).Key;
                search.Entries[i] = new(key, key);
            }

            searchVersion = IconDatabase.Version;
        }
        protected override string GetEmpty() => STRING_EMPTY;
        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys()
        {
            if (search == null || searchVersion != IconDatabase.Version)
            {
                Rebuild();
            }

            return search;
        }
        protected override Object GetAsset(string key) => IconDatabase.GetSprite(MaterialDatabase.GetIDIndex(key));
        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
