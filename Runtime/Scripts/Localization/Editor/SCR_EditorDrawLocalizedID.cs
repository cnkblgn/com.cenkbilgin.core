using UnityEditor;
using Core.Editor;

namespace Core.Localization.Editor
{
    [CustomPropertyDrawer(typeof(LocalizedID))]
    public class EditorDrawLocalizedID : EditorDrawSearchable
    {
        protected override string[] GetKeys() => LocalizationDatabase.GetKeys();
        protected override string GetKey() => "key";
    }
}