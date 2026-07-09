using UnityEditor;
using Core.Editor;

namespace Core.Audio.Editor
{
    [CustomPropertyDrawer(typeof(SoundID))]
    public class EditorDrawLocalizedID : EditorDrawSearchable
    {
        protected override string[] GetKeys() => SoundDatabase.GetIDs();
        protected override string GetKey() => "key";
        protected override void OnApply(SerializedProperty property, string key)
        {
            SerializedProperty prop = property.FindPropertyRelative("index");

            prop.intValue = SoundDatabase.GetIndex(new(key, -1));
        }
    }
}