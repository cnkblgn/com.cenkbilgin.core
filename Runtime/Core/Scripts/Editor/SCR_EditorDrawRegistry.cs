using UnityEditor;

namespace Core.Editor
{
    [CustomEditor(typeof(Registry), true)]
    internal sealed class EditorDrawRegistry : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Registry registry = target as Registry;

            EditorUtility.DrawButton("Reload", registry, registry.Reload);
        }
    }
}