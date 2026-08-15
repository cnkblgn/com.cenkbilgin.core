using UnityEditor;
using UnityEngine;
using Core.Editor;

namespace Core.Graphics.Editor
{
    using static CoreUtility;

    [CustomPropertyDrawer(typeof(MeshID))]
    public sealed class EditorDrawSearchableMeshID : EditorDrawSearchable<string>
    {
        private static SearchCollection<string> search;

        private static void Rebuild()
        {
            int meshes = MeshDatabase.GetMeshes();
            search = new SearchCollection<string>(new SearchEntry<string>[meshes]);

            for (int i = 0; i < meshes; i++)
            {
                string key = MeshDatabase.GetID(i).Key;
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
        protected override Object GetAsset(string key) => MeshDatabase.GetMesh(MeshDatabase.GetIDIndex(key));
        protected override string GetValue(SerializedProperty keyProperty) => keyProperty.stringValue;
        protected override void SetValue(SerializedProperty keyProperty, string value) => keyProperty.stringValue = value;
    }
}
