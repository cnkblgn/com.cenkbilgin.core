using System.Collections.Generic;
using UnityEditor;
using Core.Editor;

namespace Core.Actors.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(ActorTag))]
    internal sealed class EditorDrawSearchableActorTag : EditorDrawSearchable<string>
    {
        private static SearchCollection<string> search;

        private static void Rebuild()
        {
            IReadOnlyList<ActorTag> tags = ActorDatabase.GetTags();
            search = new SearchCollection<string>(new SearchEntry<string>[tags.Count]);

            for (int i = 0; i < tags.Count; i++)
            {
                string key = tags[i].Key;
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
