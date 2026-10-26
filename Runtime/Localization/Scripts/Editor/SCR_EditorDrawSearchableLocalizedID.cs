using Core.Editor;
using System.Collections.Generic;
using UnityEditor;

namespace Core.Localization.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(LocalizedID))]
    public sealed class EditorDrawSearchableLocalizedID : EditorDrawSearchable<string>
    {
        private static SearchCollection<string> search;
        private static int searchVersion = -1;

        private static void Rebuild()
        {
            IReadOnlyCollection<string> keys = LocalizationDatabase.GetKeys();
            search = new SearchCollection<string>(new SearchEntry<string>[keys.Count]);

            int i = 0;

            foreach (string key in keys)
            {
                search.Entries[i++] = new(key, key);
            }

            searchVersion = LocalizationDatabase.Version;
        }
        protected override string GetEmpty() => STRING_EMPTY;
        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys()
        {
            if (search == null || searchVersion != LocalizationDatabase.Version)
            {
                Rebuild();
            }

            return search;
        }
        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}