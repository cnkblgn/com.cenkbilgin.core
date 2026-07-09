using Core.Editor;
using System.Collections.Generic;
using UnityEditor;

namespace Core.Audio.Editor
{
    [CustomPropertyDrawer(typeof(SoundID))]
    public class EditorDrawLocalizedID : EditorDrawSearchable
    {
        protected override IReadOnlyList<string> GetKeys() => SoundDatabase.GetIDs();
        protected override string GetKey() => "key";
        protected override void OnApply(SerializedProperty property, string key)
        {
            SerializedProperty prop = property.FindPropertyRelative("index");

            prop.intValue = SoundDatabase.GetIndex(new(key, -1));
        }
    }
}