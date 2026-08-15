using Core.Editor;
using System.Collections.Generic;
using UnityEditor;

namespace Core.Damage.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(DamageTag))]
    internal sealed class EditorDrawSearchableDamageTag : EditorDrawSearchable<string>
    {
        private static SearchCollection<string> search;

        private static void Rebuild()
        {
            IReadOnlyList<DamageTag> tags = DamageDatabase.GetTags();
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
