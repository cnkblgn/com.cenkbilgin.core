using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Core.Editor;

namespace Core.Graphics.Editor
{
    [CustomPropertyDrawer(typeof(IconID))]
    public class EditorDrawIconID : EditorDrawSearchable
    {
        protected override IReadOnlyList<string> GetKeys() => IconDatabase.GetIDs();
        protected override string GetKey() => "key";
        protected override void OnApply(SerializedProperty property, string key) { }
        protected override Object GetAsset(string key) => IconDatabase.GetSprite(new(key));
    }
}