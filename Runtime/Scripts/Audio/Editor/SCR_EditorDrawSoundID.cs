using UnityEditor;
using Core.Editor;

namespace Core.Audio.Editor
{
    [CustomPropertyDrawer(typeof(SoundID))]
    public class EditorDrawLocalizedID : EditorDrawSearchable
    {
        protected override string[] GetKeys() => SoundDatabase.GetIDs();
        protected override string GetKey() => "key";
    }
}