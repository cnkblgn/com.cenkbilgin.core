using System.Collections.Generic;
using UnityEditor;
using Core.Editor;

namespace Core.Race.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(RaceID))]
    internal sealed class EditorDrawSearchableRaceID : EditorDrawSearchable<string>
    {
        private static SearchCollection<string> search;
        private static int searchVersion = -1;

        private static void Rebuild()
        {
            IReadOnlyList<RaceDefinition> definitions = RaceDatabase.GetDefinitions();
            search = new SearchCollection<string>(new SearchEntry<string>[definitions.Count]);

            for (int i = 0; i < definitions.Count; i++)
            {
                string key = definitions[i].ID.Key;
                search.Entries[i] = new(key, key);
            }

            searchVersion = RaceDatabase.Version;
        }
        protected override string GetEmpty() => STRING_EMPTY;
        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys()
        {
            if (search == null || searchVersion != RaceDatabase.Version)
            {
                Rebuild();
            }

            return search;
        }
        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
