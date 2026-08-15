using Core.Editor;
using System.Collections.Generic;
using UnityEditor;

namespace Core.Audio.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(SoundID))]
    internal sealed class EditorDrawSearchableSoundID : EditorDrawSearchable<string>
    {
        private static SearchCollection<string> search;

        private static void Rebuild()
        {
            int clips = SoundDatabase.GetClips();
            search = new SearchCollection<string>(new SearchEntry<string>[clips]);

            for (int i = 0; i < clips; i++)
            {
                string key = SoundDatabase.GetID(i).Key;
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