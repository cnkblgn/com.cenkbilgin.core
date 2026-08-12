using UnityEditor;
using UnityEngine;
using Core.Editor;

namespace Core.Graphics.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(MeshID))]
    public sealed class EditorDrawSearchableMeshID : EditorDrawSearchable<string>
    {
        protected override string GetEmpty() => STRING_EMPTY;

        protected override string GetKey() => "key";
        protected override SearchCollection<string> GetKeys() => MeshDatabase.GetIDs();

        protected override Object GetAsset(string key) => MeshDatabase.GetMesh(MeshDatabase.GetIDIndex(key));

        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
