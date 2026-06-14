using UnityEditor;
using Core.Editor;

namespace Core.Localization.Editor
{
    [CustomPropertyDrawer(typeof(LocalizedString))]
    public class EditorDrawLocalizedString : EditorDrawKey
    {
        protected override string[] GetIDs() => EditorUtilityLocalization.GetDatabase().GetKeys();
        protected override string GetKey() => "key";
    }
}