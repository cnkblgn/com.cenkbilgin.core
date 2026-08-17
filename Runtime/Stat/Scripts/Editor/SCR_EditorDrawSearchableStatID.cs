using System.Collections.Generic;
using UnityEditor;
using Core.Editor;

namespace Core.Stat.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(StatID))]
    internal sealed class EditorDrawSearchableStatID : EditorDrawSearchable<string>
    {
        private static SearchCollection<string> search;

        private static void Rebuild()
        {
            IReadOnlyList<StatDefinition> definitions = StatDatabase.GetDefinitions();
            search = new SearchCollection<string>(new SearchEntry<string>[definitions.Count]);

            for (int i = 0; i < definitions.Count; i++)
            {
                string key = definitions[i].ID.Key;
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
        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
