using UnityEditor;
using Core.Editor;

namespace Core.Audio.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(SoundID))]
    internal sealed class EditorDrawSearchableSoundID : EditorDrawSearchable<string>
    {
        private static SearchCollection<string> search;
        private static int searchVersion = -1;

        private static void Rebuild()
        {
            int clips = SoundDatabase.GetClips();
            search = new SearchCollection<string>(new SearchEntry<string>[clips]);

            for (int i = 0; i < clips; i++)
            {
                string key = SoundDatabase.GetID(i).Key;
                search.Entries[i] = new(key, key);
            }

            searchVersion = SoundDatabase.Version;
        }
        protected override string GetEmpty() => STRING_EMPTY;
        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys()
        {
            if (search == null || searchVersion != SoundDatabase.Version)
            {
                Rebuild();
            }

            return search;
        }
        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}