using Core.Editor;
using System.Collections.Generic;
using UnityEditor;

namespace Core.Localization.Editor
{
    [CustomPropertyDrawer(typeof(LocalizedID))]
    public class EditorDrawLocalizedID : EditorDrawSearchable
    {
        protected override IReadOnlyList<string> GetKeys() => LocalizationDatabase.GetKeys();
        protected override string GetKey() => "key";
    }
}