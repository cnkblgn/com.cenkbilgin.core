using System.Collections.Generic;
using UnityEditor;
using Core.Editor;

namespace Core.Event.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(EventID))]
    internal sealed class EditorDrawSearchableEventID : EditorDrawSearchable<string>
    {
        private static SearchCollection<string> search;

        private static void Rebuild()
        {
            IReadOnlyList<EventID> ids = EventDatabase.GetIDs();
            search = new SearchCollection<string>(new SearchEntry<string>[ids.Count]);

            for (int i = 0; i < ids.Count; i++)
            {
                string key = ids[i].Key;
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
