using System.Collections.Generic;
using UnityEditor;
using Core.Editor;

namespace Core.Actors.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(ActorID))]
    internal sealed class EditorDrawSearchableActorID : EditorDrawSearchable<string>
    {
        private static SearchCollection<string> search;

        private static void Rebuild()
        {
            IReadOnlyList<ActorGroup> groups = ActorDatabase.GetGroups();
            search = new SearchCollection<string>(new SearchEntry<string>[groups.Count]);

            for (int i = 0; i < groups.Count; i++)
            {
                string key = groups[i].ID.Key;
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
